using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class TrainUpgradeTroopUI : MonoBehaviour
{
    
    private PassiveResource passiveResource;

    public GameObject gameObjectParent;
    private PlayerData playerData;
    private PlayerSO playerSO;
    private PlayerBattleSO playerBattleSO;

    public UnitSO.UnitType unitTrainingType; 

    public GameObject unitTrainingPanel;
    public GameObject unitTrainingButtonPrefab;
    public TextMeshProUGUI unitTrainingPanelTitle;
    public Transform viewPort;

    public GameObject confirmPanel;
    public GameObject mainPanel;
    
    public List<UnitSO> unitList;

    public List<UnitSO> playerUnits;


    
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
        unitTrainingType = gameObjectParent.GetComponentInChildren<BuildingTrainingType>()?.unitTrainingType ?? UnitSO.UnitType.Melee;
        playerUnits = playerBattleSO.playerUnitStats;
        unitList = playerSO.unlockedUnits;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void openUnitAddPanel()//opens list
    {
       fillTrainingPanel("Train Units");
    }

    public void openUnitUpgradePanel()//opens list
    {
        fillTrainingPanel("Upgrade Units");
    }

    public void fillTrainingPanel(string text)
    {
        if (unitTrainingPanel != null)
        {
            unitTrainingPanel.SetActive(true);
            for (int i = 0; i < viewPort.childCount; i++)
            {
                Destroy(viewPort.GetChild(i).gameObject);
            }
            for (int i = 0; i < unitList.Count; i++)
            {
                if (unitList[i].unitType == unitTrainingType)
                {
                    GameObject button = Instantiate(unitTrainingButtonPrefab, viewPort);
                    button.GetComponent<TrainTroopsButton>().unitToTrain = unitList[i];
                }
            }
            unitTrainingPanelTitle.text = text;
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
        playerBattleSO.playerUnitStats = playerUnits;
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

        playerUnits.Add(unitList[0]);
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
        closeConfirmPanel();
        closeUnitTrainingPanel();
        Destroy(gameObject);
    }

    public void destroyObject()
    {
        passiveResource.refundStats();
        closeTrainTroopPanel();
        Destroy(gameObjectParent);
    }

    public void openConfirmPanel()
    {
        confirmPanel.SetActive(true);
        mainPanel.SetActive(false);
    }

    public void closeConfirmPanel()
    {
        confirmPanel.SetActive(false);
        mainPanel.SetActive(true);
    }
    
}
