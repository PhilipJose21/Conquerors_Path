using UnityEngine;
using System.Collections.Generic;

public class UnitButtonManager : MonoBehaviour
{
    private PlayerData playerData;
    private PlayerBattleSO playerBattleSO;
    private BuildingSystem buildingSystem;

    public GameObject unitButtonPrefab;
    public Transform unitButtonContainer;

    public BuildingData[] playerUnits;

    void Start()
    {
        playerData = Object.FindFirstObjectByType<PlayerData>();
        playerBattleSO = playerData.playerBattleSO;
        buildingSystem = Object.FindFirstObjectByType<BuildingSystem>();
        playerUnits = playerBattleSO.playerUnits;
        
        CreateUnitButtons();
    }

    public void RefreshUnitButtons()
    {
        foreach (Transform child in unitButtonContainer)
        {
            Destroy(child.gameObject);
        }

        List<BuildingData> activeUnits = buildingSystem != null ? buildingSystem.GetLiveBuildings() : new List<BuildingData>();
        if (activeUnits == null && playerBattleSO != null)
        {
            activeUnits = new List<BuildingData>(playerBattleSO.playerUnits);
        }

        if (activeUnits == null && playerBattleSO != null)
        {
           activeUnits = new List<BuildingData>(playerBattleSO.playerUnits);
        }

        if (activeUnits == null) return;

        Dictionary<UnitSO, int> unitCounts = new Dictionary<UnitSO, int>();

        Dictionary<UnitSO, BuildingData> unitBuildingData = new Dictionary<UnitSO, BuildingData>();

        foreach (BuildingData building in activeUnits)
        {

            if (building == null || building.unitPrefab == null) continue;

            UnitSO currentUnit = building.unitPrefab;
            if (unitCounts.ContainsKey(currentUnit))
            {
                unitCounts[currentUnit]++;
            }
            else
            {
                unitCounts[currentUnit] = 1;
                unitBuildingData[currentUnit] = building;
            }
        }

        foreach (KeyValuePair<UnitSO, int> entry in unitCounts)
        {
            UnitSO uniqueUnit = entry.Key;
            int totalCount = entry.Value;
            BuildingData correspondingBuilding = unitBuildingData[uniqueUnit];

            GameObject buttonObj = Instantiate(unitButtonPrefab, unitButtonContainer);
            UnitButton unitButtonScript = buttonObj.GetComponent<UnitButton>();

            unitButtonScript.buildingData = correspondingBuilding;
            unitButtonScript.unitData = uniqueUnit;
            unitButtonScript.unitCount = totalCount;
        }

    }

    public void CreateUnitButtons()
    {
        Dictionary<UnitSO, int> unitCounts = new Dictionary<UnitSO, int>();

        Dictionary<UnitSO, BuildingData> unitBuildingData = new Dictionary<UnitSO, BuildingData>();

        foreach (BuildingData building in playerUnits)
        {

            if (building == null || building.unitPrefab == null) continue;

            UnitSO currentUnit = building.unitPrefab;
            if (unitCounts.ContainsKey(currentUnit))
            {
                 unitCounts[currentUnit]++;
            }
            else
            {
                unitCounts[currentUnit] = 1;
                unitBuildingData[currentUnit] = building;
            }
        }

        foreach (KeyValuePair<UnitSO, int> entry in unitCounts)
        {
            UnitSO uniqueUnit = entry.Key;
            int totalCount = entry.Value;
            BuildingData correspondingBuilding = unitBuildingData[uniqueUnit];

            GameObject buttonObj = Instantiate(unitButtonPrefab, unitButtonContainer);
            UnitButton unitButtonScript = buttonObj.GetComponent<UnitButton>();

            unitButtonScript.buildingData = correspondingBuilding;
            unitButtonScript.unitData = uniqueUnit;
            unitButtonScript.unitCount = totalCount;
        }
    }
}
