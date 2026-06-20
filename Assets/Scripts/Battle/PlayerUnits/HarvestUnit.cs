using UnityEngine;

public class HarvestUnit : MonoBehaviour
{
    private UnitSO unitData;
    private MoveUnit moveUnit;
    public int harvestAmount;

    void Awake()
    {
        UnitSOContainer container = this.GetComponent<UnitSOContainer>();
        if (container != null)
        {
            unitData = container.unitData;
            moveUnit = this.GetComponent<MoveUnit>();
            
            harvestAmount = unitData != null ? unitData.harvestAmount : harvestAmount;
            // don't cache attackPoints here; read `moveUnit.attackActions` at attack time
        }
    }

    public bool TryToHarvestPosition(Vector3 worldPos)
    {
        float checkRadius = 0.6f; // small radius to detect enemy colliders inside the cell
        Collider[] hits = Physics.OverlapSphere(worldPos, checkRadius);
        foreach (var h in hits)
        {
            // Try to find a TerrainHarvest on the collider or one of its parents (covers child colliders)
            var harvest = h.GetComponentInParent<TerrainHarvest>();
            if (harvest != null)
            {
                // Ensure this harvest belongs to an enemy unit
                var owner = harvest.gameObject;
                bool isTerrain = owner.CompareTag("Terrain") || owner.GetComponentInParent<TerrainHarvest>() != null;
                if (!isTerrain) continue;

                if (moveUnit != null && moveUnit.attackActions > 0)
                {
                    harvest.HarvestResource(harvestAmount);
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
            var harvest = h.GetComponentInParent<TerrainHarvest>();
            if (harvest == null) continue;
            var owner = harvest.gameObject;
            bool isTerrain = owner.CompareTag("Terrain") || owner.GetComponentInParent<TerrainHarvest>() != null;
            if (!isTerrain) continue;

            if (moveUnit != null && moveUnit.attackActions > 0)
            {
                harvest.HarvestResource(harvestAmount);
                CellHighlighter.Instance?.ClearHighlights();
                moveUnit.attackActions = Mathf.Max(0, moveUnit.attackActions - 1);
                return;
            }
        }
    }
}
