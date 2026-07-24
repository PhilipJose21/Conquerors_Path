using UnityEngine;

public class HarvestResource : MonoBehaviour
{
    private UnitSO unitData;
    private MoveUnit moveUnit;
    public int harvestAmount;
    //allo govener
    void Awake()
    {
        UnitSOContainer container = this.GetComponent<UnitSOContainer>();
        if (container != null)
        {
            unitData = container.unitData;
            moveUnit = this.GetComponent<MoveUnit>();
            
            harvestAmount = unitData != null ? unitData.harvestAmount : harvestAmount;
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

    // Try to harvest any resource at the given world position (e.g., clicked tile).
    // Returns true if a resource was found and "harvested".
    public bool TryHarvestAtPosition(Vector3 worldPos)
    {
        float checkRadius = 0.6f; // small radius to detect resource colliders inside the cell
        Collider[] hits = Physics.OverlapSphere(worldPos, checkRadius);
        foreach (var h in hits)
        {
            // Try to find a ResourceNode on the collider or one of its parents (covers child colliders)
            var resourceNode = h.GetComponentInParent<TerrainHarvest>();
            if (resourceNode != null)
            {
                if (moveUnit != null && moveUnit.attackActions > 0)
                {
                    resourceNode.HarvestResource(harvestAmount);
                    moveUnit.attackActions = Mathf.Max(0, moveUnit.attackActions - 1);
                }
                else
                {
                    // If no MoveUnit present, still attempt with local harvestAmount fallback
                    resourceNode.HarvestResource(harvestAmount);
                }

                ActionText.Instance?.ShowHarvestText(GetDisplayName(moveUnit != null ? moveUnit.gameObject : gameObject), harvestAmount, resourceNode.transform.position);

                CellHighlighter.Instance?.ClearHighlights();
                return true;
            }
        }
        return false;
    }
}