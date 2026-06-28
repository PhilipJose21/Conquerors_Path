using UnityEngine;

public class AttackEnemyUnit : MonoBehaviour
{
    private UnitSO unitData;
    private MoveUnit moveUnit;
    private UnitSOContainer unitContainer;
    public int dmg;

    void Awake()
    {
        unitContainer = this.GetComponent<UnitSOContainer>();
        moveUnit = this.GetComponent<MoveUnit>();
        SyncFromContainer();
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

    // Try to attack any enemy at the given world position (e.g., clicked tile).
    // Returns true if an enemy was found and "attacked".
    public bool TryAttackAtPosition(Vector3 worldPos)
    {
        float checkRadius = 0.6f; // small radius to detect enemy colliders inside the cell
        Collider[] hits = Physics.OverlapSphere(worldPos, checkRadius);
        foreach (var h in hits)
        {
            // Try to find a UnitHealth on the collider or one of its parents (covers child colliders)
            var health = h.GetComponentInParent<UnitHealth>();
            if (health != null)
            {
                // Ensure this health belongs to an enemy unit
                var owner = health.gameObject;
                bool isEnemy = owner.CompareTag("EnemyUnit") || owner.GetComponentInParent<EnemyMovement>() != null;
                if (!isEnemy) continue;

                // If the defender is on a terrain that grants attack-range immunity,
                // require the attacker to be adjacent (one cell away).
                bool defenderImmune = false;
                Collider[] terrainHits = Physics.OverlapSphere(owner.transform.position, 0.2f);
                foreach (var th in terrainHits)
                {
                    var ti = th.GetComponentInParent<TerrainInteraction>();
                    if (ti != null && ti.IsAttackRangeImmune()) { defenderImmune = true; break; }
                }

                Vector3 attackerPos = moveUnit != null ? moveUnit.transform.position : transform.position;
                if (defenderImmune)
                {
                    // Check adjacency using the grid if available
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
                        // Fallback: treat within one world unit as adjacent
                        adjacent = Vector3.Distance(attackerPos, owner.transform.position) <= BuildingSystem.CellSize * 1.5f;
                    }

                    if (!adjacent)
                    {
                        Debug.Log("Target is protected by terrain (attack-range immune). Must move adjacent to attack.");
                        // Do NOT consume attack action
                        return false;
                    }
                }

                if (moveUnit != null && moveUnit.attackActions > 0)
                {
                    health.TakeDamage(dmg);
                    moveUnit.attackActions = Mathf.Max(0, moveUnit.attackActions - 1);
                }
                else
                {
                    // If no MoveUnit present, still attempt with local attackPoints fallback
                }

                CellHighlighter.Instance?.ClearHighlights();
                return true;
            }
        }
        return false;
    }

    // Scan around this unit for enemies within the unit's attack range (in cells).
    // attackRangeCells is the number of cells (Manhattan/square as defined by your rules).
    public void CheckForEnemiesInRange(int attackRangeCells, float cellSize)
    {
        float radius = (attackRangeCells + 0.5f) * cellSize;
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var h in hits)
        {
            var health = h.GetComponentInParent<UnitHealth>();
            if (health == null) continue;
            var owner = health.gameObject;
            bool isEnemy = owner.CompareTag("EnemyUnit") || owner.GetComponentInParent<EnemyMovement>() != null;
            if (!isEnemy) continue;

            if (moveUnit != null && moveUnit.attackActions > 0)
            {
                health.TakeDamage(dmg);
                CellHighlighter.Instance?.ClearHighlights();
                moveUnit.attackActions = Mathf.Max(0, moveUnit.attackActions - 1);
                return;
            }
        }
    }
}