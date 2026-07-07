using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

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
    public GameObject unitTrainingCostPrefab;

    public TextMeshProUGUI unitTrainingPanelTitle;
    public Transform viewPort;
    public Transform viewPortCost;

    public GameObject confirmPanel;
    public GameObject mainPanel;
    
    public List<UnitSO> unitList;

    public List<UnitSO> playerUnits;

    private TrainTroopsButton pendingTroopAction;

    void Awake()
    {
        closeConfirmPanel();
    }
    
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

    public void openUnitAddPanel()//opens list
    {
       fillTrainingPanel("Train", true, false);
       
    }

    public void openUnitUpgradePanel()//opens list
    {
        fillTrainingPanel("Upgrade", false, true);
    }

    public void fillTrainingPanel(string text, bool isTraining, bool isUpgrading)
    {
        if (unitTrainingPanel != null)
        {
            pendingTroopAction = null;
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
                    TrainTroopsButton trainButton = button.GetComponent<TrainTroopsButton>();
                    trainButton.unitToTrain = unitList[i];
                    trainButton.confirmationPanel = confirmPanel;
                    trainButton.trainUpgradeTroopUI = this;
                    trainButton.isTraining = isTraining;
                    trainButton.isUpgrading = isUpgrading;
                }
            }
            unitTrainingPanelTitle.text = text;
        }
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
        createCostList(pendingTroopAction.unitToTrain, viewPortCost);
        mainPanel.SetActive(false);
    }

    public void SetPendingTroopAction(TrainTroopsButton troopButton)
    {
        pendingTroopAction = troopButton;
        openConfirmPanel();
    }

    public void closeConfirmPanel()
    {
        confirmPanel.SetActive(false);
        mainPanel.SetActive(true);
        pendingTroopAction = null;
    }

    public void TRAINorUPGRADE()
    {
        if (pendingTroopAction == null)
        {
            Debug.LogWarning("TrainUpgradeTroopUI.TRAINorUPGRADE: No troop action selected.");
            closeConfirmPanel();
            return;
        }

        pendingTroopAction.TrainUnit();
        closeConfirmPanel();
    }
    
    public void createCostList(UnitSO unit, Transform costParent)
    {
        if (unit == null || unit.buildingData == null)
        {
            return;
        }

        for (int i = 0; i < costParent.childCount; i++)
        {
            Destroy(costParent.GetChild(i).gameObject);
        }
        for (int i = 0; i < 4; i++)
        {
            GameObject costPrefab = Instantiate(unitTrainingCostPrefab, costParent);
            TextMeshProUGUI costText = costPrefab.GetComponentInChildren<TextMeshProUGUI>();
            Image costImage = costPrefab.GetComponentInChildren<Image>();
            int costValue = 0;

            if (pendingTroopAction.isTraining)
            {
                switch (i)
                {
                    case 0:
                        costValue = unit.buildingData.woodCost;
                        // costImage.sprite = playerSO.woodIcon;
                        break;
                    case 1:
                        costValue = unit.buildingData.rockCost;
                        // costImage.sprite = playerSO.stoneIcon;
                        break;
                    case 2:
                        costValue = unit.buildingData.farmCost;
                        // costImage.sprite = playerSO.farmIcon;
                        break;
                    case 3:
                        costValue = unit.buildingData.coinCost;
                        // costImage.sprite = playerSO.coinIcon;
                        break;
                    default:
                        Debug.LogWarning("TrainUpgradeTroopUI.createCostList: Invalid index for resource type.");
                        break;
                }
            }
            else if (pendingTroopAction.isUpgrading)
            {
                switch (i)
                {
                    case 0:
                        costValue = unit.buildingData.woodCost * unit.level;
                        // costImage.sprite = playerSO.woodIcon;
                        break;
                    case 1:
                        costValue = unit.buildingData.rockCost * unit.level;
                        // costImage.sprite = playerSO.stoneIcon;
                        break;
                    case 2:
                        costValue = unit.buildingData.farmCost * unit.level;
                        // costImage.sprite = playerSO.farmIcon;
                        break;
                    case 3:
                        costValue = unit.buildingData.coinCost * unit.level;
                        // costImage.sprite = playerSO.coinIcon;
                        break;
                    default:
                        Debug.LogWarning("TrainUpgradeTroopUI.createCostList: Invalid index for resource type.");
                        break;
                }
            }
            

            if (costValue <= 0)
            {
                Destroy(costPrefab);
                continue;
            }

            costText.text = costValue.ToString();
        }
    }
}
