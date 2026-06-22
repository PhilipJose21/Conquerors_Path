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
            // don't cache harvestActions here; read `moveUnit.harvestActions` at harvest time
        }
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
