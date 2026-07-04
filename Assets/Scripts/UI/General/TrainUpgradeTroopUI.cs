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
        mainPanel.SetActive(false);
    }

    public void closeConfirmPanel()
    {
        confirmPanel.SetActive(false);
        mainPanel.SetActive(true);
    }
    
}
