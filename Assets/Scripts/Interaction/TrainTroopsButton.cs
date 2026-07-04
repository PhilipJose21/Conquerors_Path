using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class TrainTroopsButton : MonoBehaviour
{

    //MAKE IS INSTANTIATE AND ATTATCH THE UNITSO HERE WHEN CLICKING THE OPEN UNIT LIST
    public UnitSO unitToTrain;
    public TextMeshProUGUI unitNameText;
    public Image unitIconImage;

    public UnitSO.UnitType unitType;

    public GameObject confirmationPanel;
    public TrainUpgradeTroopUI trainUpgradeTroopUI;
    private PlayerData playerData;
    private PlayerSO playerSO;
    private PlayerBattleSO playerBattleSO;
    private List<UnitSO> playerUnits;
    private BuildingData unitCost;
    
    public bool isUpgrading = false;
    public bool isTraining = false;

    void Start()
    {
        playerData = Object.FindFirstObjectByType<PlayerData>();
        if (playerData == null)
        {
            Debug.LogError("TrainTroopsButton: PlayerData was not found in scene.");
            return;
        }

        if (unitToTrain == null)
        {
            Debug.LogError("TrainTroopsButton: unitToTrain is not assigned.");
            return;
        }

        unitNameText.text = unitToTrain.unitName;
        unitType = unitToTrain.unitType;
        playerSO = playerData.playerSO;
        playerBattleSO = playerData.playerBattleSO;
        if (playerBattleSO != null && playerBattleSO.playerUnitStats == null)
        {
            playerBattleSO.playerUnitStats = new List<UnitSO>();
        }

        playerUnits = playerBattleSO != null ? playerBattleSO.playerUnitStats : null;
        unitCost = unitToTrain.buildingData;
        unitIconImage.sprite = unitToTrain.unitIcon;
        if (unitToTrain.unitIcon == null)
        {
            Debug.LogWarning("TrainUpgradeTroopUI.fillTrainingPanel: Unit icon is not assigned for unit: " + unitToTrain.unitName);
        }
    }

    public void openConfirmationPanel()
    {
        if (trainUpgradeTroopUI != null)
        {
            trainUpgradeTroopUI.SetPendingTroopAction(this);
            return;
        }

        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
        }
    }

    public void TrainUnit()
    {
        if (isUpgrading)
        {
            checkResources();
            return;
        }

        if (isTraining)
        {
            addUnit();
            return;
        }

        Debug.LogWarning("TrainTroopsButton.TrainUnit: No action mode selected.");
    }

    public void addUnit()
    {
        if (playerBattleSO == null || playerUnits == null)
        {
            Debug.LogWarning("TrainTroopsButton.addUnit: Player unit list is not initialized.");
            return;
        }

        if (unitToTrain == null)
        {
            Debug.LogWarning("TrainTroopsButton.addUnit: unitToTrain is not assigned.");
            return;
        }

        checkUnitCost(unitToTrain);
    }

    public void checkUnitCost(UnitSO unit)
    {
        if (unit == null || unit.buildingData == null)
        {
            Debug.LogWarning("TrainTroopsButton.checkUnitCost: Unit or unit cost data is missing.");
            return;
        }

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

        playerUnits.Add(unit);
        updatePlayerUnits();
        playerData.updateUnitList();
        Debug.Log("Unit trained: " + unit.unitName);
    }

    public void updatePlayerUnits()
    {
        playerBattleSO.playerUnitStats = playerUnits;
    }

    public void checkResources()
    {
        if (unitToTrain.level > 10)
        {
            return;
        }

        if (unitCost.woodCost <= playerSO.woodResources && unitCost.rockCost <= playerSO.stoneResources && unitCost.farmCost <= playerSO.farmResources && unitCost.coinCost <= playerSO.coins)
        {
            playerSO.woodResources -= unitCost.woodCost;
            playerSO.stoneResources -= unitCost.rockCost;
            playerSO.farmResources -= unitCost.farmCost;
            playerSO.coins -= unitCost.coinCost;

            Debug.Log("Unit Trained");
            unitToTrain.level += 1;
            checkType();
        }
        else
        {
            Debug.Log("Not enough resources to train unit.");
        }
    }

    public void checkType()
    {
        switch(unitType)
        {
            case UnitSO.UnitType.Melee:
                increaseMeleeStats(unitToTrain.level);
                break;
            case UnitSO.UnitType.Ranger:
                increaseRangerStats(unitToTrain.level);
                break;
            case UnitSO.UnitType.Support:
                increaseSupportStats(unitToTrain.level);
                break;
        }

    }

    public void increaseMeleeStats(int level)
    {
        if (level == 2 || level == 3)
        {
            unitToTrain.damage += 1;
        }

        else if (level == 4 || level == 6)
        {
            unitToTrain.health += 1;
        }

        else if (level == 5 || level == 7)
        {
            unitToTrain.damage += 2;
        }

        else if (level == 8 || level == 9)
        {
            unitToTrain.damage += 1;
            unitToTrain.health += 1;
        }

        else if (level == 10)
        {
            unitToTrain.attackRange += 1;
            unitToTrain.mobility += 1;
        }
        Debug.Log("MELEE SUCCESS");
    }

    public void increaseRangerStats(int level)
    {
        if (level == 2 || level == 3 || level == 6)
        {
            unitToTrain.damage += 1;
        }
        else if (level == 4 || level == 7)
        {
            unitToTrain.mobility += 1;
        }
        else if (level == 5)
        {
            unitToTrain.health += 1;
        }
        else if (level == 8)
        {
            unitToTrain.damage += 2;
        }
        else if (level == 9 || level == 10)
        {
            unitToTrain.attackRange += 1;
        }
        Debug.Log("RANGER SUCCESS");
    }

    public void increaseSupportStats(int level)
    {
        if (level == 2 || level == 3)
        {
            unitToTrain.health += 1;
        }
        else if (level == 4 || level == 8)
        {
            unitToTrain.mobility += 1;
        }
        else if (level == 5)
        {
            unitToTrain.damage += 1;
        }
        else if (level == 6 || level == 7)
        {
            unitToTrain.health += 1;
            unitToTrain.damage += 1;
        }
        else if (level == 9)
        {
            unitToTrain.attackRange += 1;
        }
        else if (level == 10)
        {
            unitToTrain.attackPoints += 1;
            unitToTrain.movePoints += 1;
        }
        Debug.Log("SUPPORT SUCCESS");
    }

    public void increaseUnitCost(UnitSO unit)
    {

    }


}
