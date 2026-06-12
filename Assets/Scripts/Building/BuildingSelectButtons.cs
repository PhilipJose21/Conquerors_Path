using UnityEngine;

public class BuildingSelectButtons : MonoBehaviour
{
    // Simple UI helper that forwards button presses to the BuildingSystem to select
    // a building by index. Keeps the UI decoupled from BuildingSystem internals.
    public int buildingDataIndex = 0;
    private BuildingSystem buildingSystem => Object.FindAnyObjectByType<BuildingSystem>();

    public void SelectBuilding()
    {
        if (buildingSystem != null)
        {
            buildingSystem.SelectBuilding(buildingDataIndex);
            var data = buildingSystem.GetBuildingData(buildingDataIndex);
            if (data != null)
            {
                KingdomUIManager.Instance?.ShowSelectedBuilding(data);
            }
        }
    }
}
