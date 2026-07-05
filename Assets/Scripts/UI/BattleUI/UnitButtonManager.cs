using UnityEngine;
using System.Collections.Generic;

public class UnitButtonManager : MonoBehaviour
{
    private PlayerData playerData;
    private PlayerBattleSO playerBattleSO;
    private BuildingSystem buildingSystem;
    private readonly List<UnitButton> spawnedButtons = new List<UnitButton>();

    public GameObject unitButtonPrefab;
    public Transform unitButtonContainer;

    public BuildingData[] playerUnits;

    void Start()
    {
        playerData = Object.FindFirstObjectByType<PlayerData>();
        playerBattleSO = playerData != null ? playerData.playerBattleSO : null;
        buildingSystem = Object.FindFirstObjectByType<BuildingSystem>();
        
        if (playerBattleSO != null && playerBattleSO.playerUnits != null)
        {
            playerUnits = playerBattleSO.playerUnits.ToArray();
        }

        // TESTING FALLBACK: If the list is empty, make sure buttons still spawn for testing
        if (playerUnits == null || playerUnits.Length == 0)
        {
            Debug.LogWarning("PlayerBattleSO units list is empty! Using raw asset array instead.");
            // If you have an array assigned directly on this script component in the Inspector:
            // playerUnits = someInspectorTestArray; 
        }
        
        CreateUnitButtons();
    }

    public void RefreshUnitButtons()
    {
        ClearButtons();

        // 1. Get live buildings from the system
        List<BuildingData> activeUnits = buildingSystem != null ? buildingSystem.GetLiveBuildings() : null;

        // 2. If activeUnits is null or empty, fall back to the main battle list
        if ((activeUnits == null || activeUnits.Count == 0) && playerBattleSO != null)
        {
            activeUnits = new List<BuildingData>(playerBattleSO.playerUnits);
        }

        if (activeUnits == null)
        {
            return;
        }

        BuildButtonsFromUnits(activeUnits);
    }

    public void CreateUnitButtons()
    {
        ClearButtons();

        if (playerUnits == null)
        {
            return;
        }

        BuildButtonsFromUnits(new List<BuildingData>(playerUnits));
    }

    public bool TryGetBuildingDataAtSlot(int index, out BuildingData buildingData)
    {
        buildingData = null;

        if (index < 0 || index >= spawnedButtons.Count)
        {
            return false;
        }

        UnitButton unitButton = spawnedButtons[index];
        if (unitButton == null)
        {
            return false;
        }

        buildingData = unitButton.buildingData;
        return buildingData != null;
    }

    private void ClearButtons()
    {
        spawnedButtons.Clear();

        foreach (Transform child in unitButtonContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void BuildButtonsFromUnits(List<BuildingData> sourceUnits)
    {
        Dictionary<UnitSO, int> unitCounts = new Dictionary<UnitSO, int>();
        Dictionary<UnitSO, BuildingData> unitBuildingData = new Dictionary<UnitSO, BuildingData>();
        List<UnitSO> orderedUnits = new List<UnitSO>();

        foreach (BuildingData building in sourceUnits)
        {
            if (building == null || building.unitPrefab == null)
            {
                continue;
            }

            UnitSO currentUnit = building.unitPrefab;
            if (unitCounts.ContainsKey(currentUnit))
            {
                unitCounts[currentUnit]++;
            }
            else
            {
                unitCounts[currentUnit] = 1;
                unitBuildingData[currentUnit] = building;
                orderedUnits.Add(currentUnit);
            }
        }

        foreach (UnitSO uniqueUnit in orderedUnits)
        {
            int totalCount = unitCounts[uniqueUnit];
            BuildingData correspondingBuilding = unitBuildingData[uniqueUnit];

            GameObject buttonObj = Instantiate(unitButtonPrefab, unitButtonContainer);
            UnitButton unitButtonScript = buttonObj.GetComponent<UnitButton>();

            unitButtonScript.buildingData = correspondingBuilding;
            unitButtonScript.unitData = uniqueUnit;
            unitButtonScript.unitCount = totalCount;
            spawnedButtons.Add(unitButtonScript);
        }
    }
}
