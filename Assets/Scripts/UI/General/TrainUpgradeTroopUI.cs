using UnityEngine;
using System.Collections.Generic;

public class TrainUpgradeTroopUI : MonoBehaviour
{
    public GameObject gameObjectParent;
    public PassiveResource passiveResource;
    public PlayerData playerData;
    private PlayerSO playerSO;
    private PlayerBattleSO playerBattleSO;
    public GameObject unitTrainingPanel;

    public List<UnitSO> unitList;

    public List<GameObject> playerUnits;

    
    void Start()
    {
        playerData = Object.FindFirstObjectByType<PlayerData>();
        passiveResource = gameObjectParent.GetComponentInChildren<PassiveResource>();
        if (playerData == null)
        {
            Debug.LogError("TrainUpgradeTroopUI: PlayerData was not found in scene.");
            return;
        }

        playerSO = playerData.playerSO;
        playerBattleSO = playerData.playerBattleSO;
        if (playerBattleSO == null)
        {
            Debug.LogError("TrainUpgradeTroopUI: PlayerBattleSO is not assigned on PlayerData.");
            return;
        }

        if (playerBattleSO.playerUnitStats == null)
        {
            playerBattleSO.playerUnitStats = new List<UnitSO>();
        }

        playerUnits = playerBattleSO.playerUnitPrefabs;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void openUnitTrainingPanel()
    {
        if (unitTrainingPanel != null)
        {
            unitTrainingPanel.SetActive(true);
        }
    }

    public void addUnit()
    {
        if (playerBattleSO == null || playerUnits == null)
        {
            Debug.LogWarning("TrainUpgradeTroopUI.addUnit: Player unit list is not initialized.");
            return;
        }

        if (unitList == null || unitList.Count == 0 || unitList[0] == null)
        {
            Debug.LogWarning("TrainUpgradeTroopUI.addUnit: unitList is empty or first unit is not assigned.");
            return;
        }
        checkUnitCost(unitList[0]);
    }

    public void updateResources()
    {
        playerSO.woodResources = playerData.playerWoodResources;
        playerSO.stoneResources = playerData.playerStoneResources;
        playerSO.farmResources = playerData.playerFarmResources;
        playerSO.energyPoints = playerData.playerEnergyPoints;
        playerSO.researchPoints = playerData.playerResearchPoints;
        playerSO.gems = playerData.playerGems;
        playerSO.coins = playerData.playerCoins;
    }

    public void updatePlayerUnits()
    {
        //EXAMINE
        playerBattleSO.playerUnitPrefabs = playerUnits;
    }

    public void checkUnitCost(UnitSO unit)
    {
        BuildingData unitResource = unit.buildingData;
        if (unitResource.woodCost > playerData.playerWoodResources 
        || unitResource.rockCost > playerData.playerStoneResources 
        || unitResource.farmCost > playerData.playerFarmResources 
        || unitResource.coinCost > playerData.playerCoins)
        {
            Debug.Log("Not enough resources to train unit: " + unit.unitName);
            return;
        }

        playerData.playerWoodResources -= unitResource.woodCost;
        playerData.playerStoneResources -= unitResource.rockCost;
        playerData.playerFarmResources -= unitResource.farmCost;
        playerData.playerCoins -= unitResource.coinCost;

        playerUnits.Add(unitList[0].buildingData.Model.gameObject);
        updatePlayerUnits();
        playerData.updateUnitList();
        Debug.Log("Success");
    }

    public void closeUnitTrainingPanel()
    {
        if (unitTrainingPanel != null)
        {
            unitTrainingPanel.SetActive(false);
        }
    }

    public void closeTrainTroopPanel()
    {
        Destroy(gameObject);
    }

    public void destroyObject()
    {
        passiveResource.refundStats();
        closeTrainTroopPanel();
        Destroy(gameObjectParent);
    }
}
