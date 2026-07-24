using UnityEngine;

public class HarvestUnit : MonoBehaviour
{
    private UnitSO unitData;
    private MoveUnit moveUnit;
    private UnitStateMachine stateMachine;
    public int harvestAmount;

    void Awake()
    {
        UnitSOContainer container = this.GetComponent<UnitSOContainer>();
        stateMachine = this.GetComponent<UnitStateMachine>();
        if (container != null)
        {
            unitData = container.unitData;
            moveUnit = this.GetComponent<MoveUnit>();
            
            harvestAmount = unitData != null ? unitData.harvestAmount : harvestAmount;
            // don't cache attackPoints here; read `moveUnit.attackActions` at attack time
        }
    }

    // Best-effort display name for combat text: prefers the unit's UnitSO name,
    // falls back to the GameObject's name if no UnitSOContainer is found.
    private string GetDisplayName(GameObject go)
    {
        if (go == null) return "Unit";
        var container = go.GetComponentInParent<UnitSOContainer>();
        if (container != null && container.unitData != null)
        {
            return container.unitData.unitName;
        }
        return go.name;
    }

    public bool TryToHarvestPosition(Vector3 worldPos, TerrainHarvest targetTerrain = null)
    {
        // If targetTerrain wasn't passed directly, fall back to searching the area with a larger radius
        if (targetTerrain == null)
        {
            float checkRadius = 0.85f; // Increased from 0.6f to reliably hit cell bounds
            Collider[] hits = Physics.OverlapSphere(worldPos, checkRadius);

            foreach (var h in hits)
            {
                var harvest = h.GetComponentInParent<TerrainHarvest>();
                if (harvest != null && harvest.canHarvest && harvest.resourceType != TerrainSO.ResourceType.None)
                {
                    targetTerrain = harvest;
                    break;
                }
            }
        }

        // Execute harvest
        if (targetTerrain != null)
        {
            if (moveUnit != null && moveUnit.attackActions > 0)
            {
                targetTerrain.HarvestResource(harvestAmount);
                moveUnit.attackActions = Mathf.Max(0, moveUnit.attackActions - 1);
                stateMachine?.ChangeState(unitPhase.Damage);
                
                ActionText.Instance?.ShowHarvestText(
                    GetDisplayName(moveUnit != null ? moveUnit.gameObject : gameObject), 
                    harvestAmount, 
                    targetTerrain.transform.position
                );
                
                CellHighlighter.Instance?.ClearHighlights();
                return true;
            }
            else
            {
                Debug.Log("Unit has no attack/harvest actions left!");
                return false;
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
            if (!harvest.canHarvest || harvest.resourceType == TerrainSO.ResourceType.None) continue;

            if (moveUnit != null && moveUnit.attackActions > 0)
            {
                harvest.HarvestResource(harvestAmount);
                CellHighlighter.Instance?.ClearHighlights();
                moveUnit.attackActions = Mathf.Max(0, moveUnit.attackActions - 1);
                stateMachine?.ChangeState(unitPhase.Damage);
                ActionText.Instance?.ShowHarvestText(GetDisplayName(moveUnit != null ? moveUnit.gameObject : gameObject), harvestAmount, harvest.transform.position);
                return;
            }
        }
    }
}