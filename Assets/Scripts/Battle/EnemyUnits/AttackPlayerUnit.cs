using UnityEngine;

public class AttackPlayerUnit : MonoBehaviour
{
    private UnitSO unitData;
    private EnemyMovement moveComp;
    private UnitSOContainer unitContainer;
    public int dmg;

    void Awake()
    {
        // Find UnitSOContainer and EnemyMovement on this or parent objects so component can live on child objects
        unitContainer = this.GetComponentInParent<UnitSOContainer>();
        SyncFromContainer();
        moveComp = this.GetComponentInParent<EnemyMovement>();
    }

    public void SyncFromContainer()
    {
        if (unitContainer == null || unitContainer.unitData == null)
        {
            return;
        }

        unitData = unitContainer.unitData;
        dmg = unitContainer.GetDamage();
    }

    // Try to attack any player at the given world position (e.g., nearest player).
    // Returns true if a player was found and attacked.
    public bool TryAttackAtPosition(Vector3 worldPos)
    {
        float checkRadius = 0.6f;
        int available = moveComp != null ? moveComp.attackActions : 0;
        Collider[] hits = Physics.OverlapSphere(worldPos, checkRadius);
        
        // Collect candidate player hits and whether they are hidden
        var candidates = new System.Collections.Generic.List<(UnitHealth health, GameObject owner, MoveUnit mover)>();
        foreach (var h in hits)
        {
            var health = h.GetComponentInParent<UnitHealth>();
            if (health == null)
            {
                continue;
            }
            var owner = health.gameObject;
            var mover = owner.GetComponentInChildren<MoveUnit>() ?? owner.GetComponentInParent<MoveUnit>();
            bool isPlayer = owner.CompareTag("PlayerUnit") || mover != null;
            if (!isPlayer) continue;
            candidates.Add((health, owner, mover));
        }

        if (candidates.Count == 0) return false;

        // If there are any unhidden players anywhere in the scene, ignore hidden candidates
        bool anyUnhiddenGlobal = false;
        var allPlayers = GameObject.FindGameObjectsWithTag("PlayerUnit");
        foreach (var p in allPlayers)
        {
            var mv = p.GetComponentInChildren<MoveUnit>() ?? p.GetComponentInParent<MoveUnit>();
            if (mv == null || !mv.isHidden) { anyUnhiddenGlobal = true; break; }
        }

        

        var targets = new System.Collections.Generic.List<(UnitHealth health, GameObject owner, MoveUnit mover)>();
        foreach (var c in candidates)
        {
            if (anyUnhiddenGlobal && c.mover != null && c.mover.isHidden)
            {
                continue;
            }
            targets.Add(c);
        }

        // If no targets after filtering:
        // - If there are unhidden players anywhere in the scene, do NOT attack hidden candidates here.
        // - Otherwise (all players hidden), allow falling back to candidates.
        //
        if (targets.Count == 0)
        {
            if (anyUnhiddenGlobal)
            {
                return false;
            }
            targets.AddRange(candidates);
        }

        foreach (var entry in targets)
        {
            var health = entry.health;
            var owner = entry.owner;
            var playerMover = entry.mover;

            // If defender stands on terrain that grants attack-range immunity,
            // require adjacency to attack.
            bool defenderImmune = false;
            Collider[] terrainHits = Physics.OverlapSphere(owner.transform.position, 0.2f);
            foreach (var th in terrainHits)
            {
                var ti = th.GetComponentInParent<TerrainInteraction>();
                if (ti != null && ti.IsAttackRangeImmune()) { defenderImmune = true; break; }
            }

            Vector3 attackerPos = moveComp != null ? moveComp.transform.position : transform.position;
            if (defenderImmune)
            {
                BuildingGrid grid = BuildingGridManager.Instance.FindGridAtPosition(owner.transform.position);
                bool adjacent = false;
                if (grid != null)
                {
                    (int ax, int ay) = grid.WorldToGridPosition(attackerPos);
                    (int dx, int dy) = grid.WorldToGridPosition(owner.transform.position);
                    adjacent = (Mathf.Abs(ax - dx) + Mathf.Abs(ay - dy)) == 1;
                }
                else
                {
                    adjacent = Vector3.Distance(attackerPos, owner.transform.position) <= BuildingSystem.CellSize * 1.5f;
                }

                if (!adjacent)
                {
                
                    return false;
                }
            }

            if (moveComp != null && moveComp.attackActions > 0)
            {
                health.TakeDamage(dmg);
                moveComp.attackActions = Mathf.Max(0, moveComp.attackActions - 1);
                return true;
            }
            else
            {
                Debug.LogWarning("Enemy tried to attack but has no attack actions left.");
            }
        }
        return false;
    }

    public void CheckForPlayersInRange(int attackRangeCells, float cellSize)
    {
        float radius = (attackRangeCells + 0.5f) * cellSize;
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var h in hits)
        {
            var health = h.GetComponentInParent<UnitHealth>();
            if (health == null)
            {
                
                continue;
            }
            var owner = health.gameObject;
            var mover = owner.GetComponentInParent<MoveUnit>();
            bool isPlayer = owner.CompareTag("PlayerUnit") || mover != null;
            if (!isPlayer) continue;

            // Gather nearby player candidates first
            // (Simpler approach here: if this particular owner is hidden but there exists any unhidden player in scene, skip it)
            bool anyUnhidden = false;
            var allPlayers = GameObject.FindGameObjectsWithTag("PlayerUnit");
            foreach (var p in allPlayers) { var mv = p.GetComponentInParent<MoveUnit>(); if (mv == null || !mv.isHidden) { anyUnhidden = true; break; } }
            if (anyUnhidden && mover != null && mover.isHidden) continue;

            if (moveComp != null && moveComp.attackActions > 0)
            {
                health.TakeDamage(dmg);
                moveComp.attackActions = Mathf.Max(0, moveComp.attackActions - 1);
                return;
            }
            else
            {
                Debug.LogWarning("Enemy tried to attack in CheckForPlayersInRange but has no attack actions left.");
            }
        }
    }
}
