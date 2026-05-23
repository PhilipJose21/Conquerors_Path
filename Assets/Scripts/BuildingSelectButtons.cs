using UnityEngine;

public class BuildingSelectButtons : MonoBehaviour
{
    public int buildingDataIndex = 0;
    private BuildingSystem buildingSystem => FindObjectOfType<BuildingSystem>();

    public void SelectBuilding()
    {
        if (buildingSystem != null)
        {
            buildingSystem.SelectBuilding(buildingDataIndex);
        }
    }
}
