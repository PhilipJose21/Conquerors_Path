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

    private ScrollRect unitTrainingScrollRect;

    private TrainTroopsButton pendingTroopAction;

    void Awake()
    {
        closeConfirmPanel();
    }
    
    void Start()
    {
        playerData = Object.FindFirstObjectByType<PlayerData>();
        passiveResource = gameObjectParent != null ? gameObjectParent.GetComponentInChildren<PassiveResource>(true) : null;
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

        unitTrainingScrollRect = unitTrainingPanel != null ? unitTrainingPanel.GetComponentInChildren<ScrollRect>(true) : null;
        if (unitTrainingScrollRect != null && unitTrainingScrollRect.content == null && viewPort is RectTransform viewportRect)
        {
            unitTrainingScrollRect.content = viewportRect;
        }

        unitTrainingType = gameObjectParent != null
            ? gameObjectParent.GetComponentInChildren<BuildingTrainingType>(true)?.unitTrainingType ?? UnitSO.UnitType.Melee
            : UnitSO.UnitType.Melee;
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
        if (unitTrainingPanel == null)
        {
            Debug.LogWarning("TrainUpgradeTroopUI.fillTrainingPanel: unitTrainingPanel is not assigned.");
            return;
        }

        pendingTroopAction = null;
        unitTrainingPanel.SetActive(true);

        if (playerSO != null)
        {
            unitList = playerSO.unlockedUnits;
        }

        if (gameObjectParent != null)
        {
            unitTrainingType = gameObjectParent.GetComponentInChildren<BuildingTrainingType>(true)?.unitTrainingType ?? unitTrainingType;
        }

        if (viewPort == null || unitTrainingButtonPrefab == null || unitTrainingPanelTitle == null)
        {
            Debug.LogError("TrainUpgradeTroopUI.fillTrainingPanel: Missing viewport, button prefab, or title reference.");
            return;
        }

        Transform buttonParent = unitTrainingScrollRect != null && unitTrainingScrollRect.content != null
            ? unitTrainingScrollRect.content
            : viewPort;

        for (int i = buttonParent.childCount - 1; i >= 0; i--)
        {
            Destroy(buttonParent.GetChild(i).gameObject);
        }

        if (unitList == null)
        {
            Debug.LogWarning("TrainUpgradeTroopUI.fillTrainingPanel: unitList is null.");
            unitTrainingPanelTitle.text = text;
            return;
        }

        for (int i = 0; i < unitList.Count; i++)
        {
            UnitSO unit = unitList[i];
            if (unit == null)
            {
                continue;
            }

            if (unit.unitType != unitTrainingType)
            {
                continue;
            }

            GameObject button = Instantiate(unitTrainingButtonPrefab, buttonParent);
            TrainTroopsButton trainButton = button.GetComponent<TrainTroopsButton>();
            if (trainButton == null)
            {
                Debug.LogError("TrainUpgradeTroopUI.fillTrainingPanel: Button prefab is missing TrainTroopsButton.");
                Destroy(button);
                continue;
            }

            trainButton.unitToTrain = unit;
            trainButton.confirmationPanel = confirmPanel;
            trainButton.trainUpgradeTroopUI = this;
            trainButton.isTraining = isTraining;
            trainButton.isUpgrading = isUpgrading;
        }

        if (buttonParent.childCount == 0)
        {
            Debug.LogWarning($"TrainUpgradeTroopUI.fillTrainingPanel: No units matched type {unitTrainingType}. Falling back to all unlocked units.");

            for (int i = 0; i < unitList.Count; i++)
            {
                UnitSO unit = unitList[i];
                if (unit == null)
                {
                    continue;
                }

                GameObject button = Instantiate(unitTrainingButtonPrefab, buttonParent);
                TrainTroopsButton trainButton = button.GetComponent<TrainTroopsButton>();
                if (trainButton == null)
                {
                    Debug.LogError("TrainUpgradeTroopUI.fillTrainingPanel: Button prefab is missing TrainTroopsButton.");
                    Destroy(button);
                    continue;
                }

                trainButton.unitToTrain = unit;
                trainButton.confirmationPanel = confirmPanel;
                trainButton.trainUpgradeTroopUI = this;
                trainButton.isTraining = isTraining;
                trainButton.isUpgrading = isUpgrading;
            }
        }

        unitTrainingPanelTitle.text = text;
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
