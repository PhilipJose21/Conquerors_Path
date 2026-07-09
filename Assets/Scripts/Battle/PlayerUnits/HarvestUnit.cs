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
        float checkRadius = 0.6f; 
        Collider[] hits = Physics.OverlapSphere(worldPos, checkRadius);

        TerrainHarvest targetTerrain = null;
        GameObject enemyUnit = null; // Placeholder for your enemy detection method

        // STEP 1: Scan all hits to categorize what is in the cell
        foreach (var h in hits)
        {
            // Check if it's an Enemy Unit (Adjust this condition to match your team/faction setup)
            if (h.CompareTag("Enemy")) 
            {
                enemyUnit = h.gameObject;
                break; // Found an enemy! We can stop looking because enemy takes top priority.
            }

            // Check if it's a Terrain Harvest source
            var harvest = h.GetComponentInParent<TerrainHarvest>();
            if (harvest != null)
            {
                var owner = harvest.gameObject;
                bool isTerrain = owner.CompareTag("Terrain") || owner.GetComponentInParent<TerrainHarvest>() != null;
                if (isTerrain)
                {
                    targetTerrain = harvest;
                }
            }
        }

        // STEP 2: Execute action based on priority
        
        // Scenario A: An Enemy is standing there -> Attack them instead of harvesting
        if (enemyUnit != null)
        {
            if (moveUnit != null && moveUnit.attackActions > 0)
            {
                // TODO: Call your attack logic here, e.g.:
                // enemyUnit.GetComponent<Health>().TakeDamage(damageAmount);
                
                moveUnit.attackActions = Mathf.Max(0, moveUnit.attackActions - 1);
                CellHighlighter.Instance?.ClearHighlights();
                return true;
            }
        }
        // Scenario B: No enemy found, but valid Terrain is present -> Harvest it (Player units ignored)
        else if (targetTerrain != null)
        {
            if (moveUnit != null && moveUnit.attackActions > 0)
            {
                targetTerrain.HarvestResource(harvestAmount);
                moveUnit.attackActions = Mathf.Max(0, moveUnit.attackActions - 1);
            }

            CellHighlighter.Instance?.ClearHighlights();
            return true;
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
