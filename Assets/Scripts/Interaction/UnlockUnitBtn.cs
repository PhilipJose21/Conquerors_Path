using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnlockUnitBtn : MonoBehaviour
{
    public UnitSO unitToUnlock;
    public TextMeshProUGUI unitNameText;
    public Image unitIconImage;

    public UnitSO.UnitType unitType;

    public GameObject confirmationPanel;
    public GameObject unitUnlockCostPrefab;
    public Transform costParent;
    public GameObject unlockTroopPanel;

    public int woodCost;
    public int rockCost;
    public int farmCost;
    public int coinCost;

    private PlayerData playerData;
    private PlayerSO playerSO;
    private PlayerBattleSO playerBattleSO;

    void Start()
    {
        playerData = Object.FindFirstObjectByType<PlayerData>();
        playerSO = playerData.playerSO;
        playerBattleSO = playerData.playerBattleSO;
    }

    public void createCostList(UnitSO unit, Transform costParent)
    {
        if (unit == null) return;
        unitNameText.text = unit.unitName;
        ResourceCostListBuilder.Build(unit.buildingData, playerSO, unitUnlockCostPrefab, costParent, 4);
    }

    public void openConfirmationPanel()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
            createCostList(unitToUnlock, costParent);
            unlockTroopPanel.GetComponent<UnlockUnits>().selectedUnit = unitToUnlock;
        }
    }

    
}
