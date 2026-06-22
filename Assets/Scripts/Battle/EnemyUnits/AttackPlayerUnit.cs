using UnityEngine;

public class AttackPlayerUnit : MonoBehaviour
{
    private UnitSO unitData;
    private EnemyMovement moveComp;
    public int dmg;

    void Awake()
    {
        // Find UnitSOContainer and EnemyMovement on this or parent objects so component can live on child objects
        UnitSOContainer container = this.GetComponentInParent<UnitSOContainer>();
        if (container != null)
        {
            unitData = container.unitData;
            dmg = unitData != null ? unitData.damage : dmg;
        }
        moveComp = this.GetComponentInParent<EnemyMovement>();
    }

    // Try to attack any player at the given world position (e.g., nearest player).
    // Returns true if a player was found and attacked.
    public bool TryAttackAtPosition(Vector3 worldPos)
    {
        float checkRadius = 0.6f;
        int available = moveComp != null ? moveComp.attackActions : 0;
        Collider[] hits = Physics.OverlapSphere(worldPos, checkRadius);
        Debug.Log($"AttackPlayerUnit: Overlap hits={hits.Length}");
        foreach (var h in hits)
        {
            // Attempt to find UnitHealth on the collider or its parents
            var health = h.GetComponentInParent<UnitHealth>();
            if (health == null)
            {
                Debug.Log("  No UnitHealth on this hit");
                continue;
            }

            var owner = health.gameObject;
            bool isPlayer = owner.CompareTag("PlayerUnit") || owner.GetComponentInParent<MoveUnit>() != null;
            Debug.Log($"  Found UnitHealth on {owner.name}, isPlayer={isPlayer}");
            if (!isPlayer) continue;

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
                    Debug.Log("Target is protected by terrain (attack-range immune). Must move adjacent to attack.");
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
            Debug.Log($"  Overlap hit: {h.gameObject.name} tag={h.gameObject.tag}");
            var health = h.GetComponentInParent<UnitHealth>();
            if (health == null)
            {
                Debug.Log("  No UnitHealth on this hit");
                continue;
            }
            var owner = health.gameObject;
            bool isPlayer = owner.CompareTag("PlayerUnit") || owner.GetComponentInParent<MoveUnit>() != null;
            if (!isPlayer) continue;

            if (moveComp != null && moveComp.attackActions > 0)
            {
                Debug.Log($"  CheckForPlayersInRange attacking {owner.name} for {dmg}");
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
