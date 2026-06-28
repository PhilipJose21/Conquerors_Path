using UnityEngine;

public class HarvestResource : MonoBehaviour
{
    private UnitSO unitData;
    private MoveUnit moveUnit;
    private UnitSOContainer unitContainer;
    public int harvestAmount;
    //allo govener
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
        harvestAmount = unitContainer.GetHarvestAmount();
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

                CellHighlighter.Instance?.ClearHighlights();
                return true;
            }
        }
        return false;
    }
}
