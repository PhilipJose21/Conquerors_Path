using UnityEngine;
using UnityEngine.UI;

public class RemoveFromBuildingSystem : MonoBehaviour
{
    public BuildingData buildingToRemove;
    private BuildingSystem buildingSystem;
    private Button button;

    void Start()
    {
        buildingSystem = FindFirstObjectByType<BuildingSystem>();
        button = GetComponent<Button>();
        RefreshAvailability();
    }

    void Update()
    {
        if (buildingSystem == null)
        {
            buildingSystem = FindFirstObjectByType<BuildingSystem>();
        }

        RefreshAvailability();
    }

    public void RemoveBuilding()
    {
        RefreshAvailability();
    }

    private void RefreshAvailability()
    {
        if (buildingSystem == null || buildingToRemove == null)
        {
            return;
        }

        bool canPlace = buildingSystem.CanPlaceBuilding(buildingToRemove);
        if (button != null)
        {
            button.interactable = canPlace;
        }
    }
}
