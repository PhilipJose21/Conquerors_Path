using UnityEngine;

public class AttackEnemyUnit : MonoBehaviour
{
    private UnitSO unitData;
    private MoveUnit moveUnit;

    void Awake()
    {
        UnitSOContainer container = this.GetComponent<UnitSOContainer>();
        if (container != null)
        {
            unitData = container.unitData;
            moveUnit = this.GetComponent<MoveUnit>();
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
            if (h.CompareTag("EnemyUnit"))
            {
                Debug.Log("Enemy Attacked");
                // Clear highlights after attack
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
            if (h.CompareTag("EnemyUnit"))
            {
                Debug.Log("Enemy Attacked");
                CellHighlighter.Instance?.ClearHighlights();
                return;
            }
        }
    }
}