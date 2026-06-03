using UnityEngine;

public class AttackEnemyUnit : MonoBehaviour
{
    private UnitSO unitData;
    private MoveUnit moveUnit;
    public int dmg;

    void Awake()
    {
        UnitSOContainer container = this.GetComponent<UnitSOContainer>();
        if (container != null)
        {
            unitData = container.unitData;
            moveUnit = this.GetComponent<MoveUnit>();
            
            dmg = unitData != null ? unitData.damage : dmg;
            // don't cache attackPoints here; read `moveUnit.attackActions` at attack time
        }
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