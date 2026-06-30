using UnityEngine;
using TMPro;

public class TrainTroopsButton : MonoBehaviour
{

    //MAKE IS INSTANTIATE AND ATTATCH THE UNITSO HERE WHEN CLICKING THE OPEN UNIT LIST
    public UnitSO unitToTrain;
    public TextMeshProUGUI unitNameText;

    public UnitSO.UnitType unitType;

    public GameObject confirmationPanel;
    private PlayerData playerData;
    private PlayerSO playerSO;
    private BuildingData unitCost;
    

    void Start()
    {
        playerData = Object.FindFirstObjectByType<PlayerData>();
        unitNameText.text = unitToTrain.unitName;
        unitType = unitToTrain.unitType;
        playerSO = playerData.playerSO;
        unitCost = unitToTrain.buildingData;
    }

    void Update()
    {

    }

    public void openConfirmationPanel()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
        }
    }

    public void TrainUnit()
    {
        checkResources();
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


}
