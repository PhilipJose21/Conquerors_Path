using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class UnlockUnits : MonoBehaviour
{
    public UnitSO selectedUnit;
    public Button meleeButton; // Button for unlocking melee units
    public Button rangedButton; // Button for unlocking ranged units
    public Button supportButton; // Button for unlocking support units

    public Image buildingIconImage; // Drag your Building Icon Image component here in the Inspector!
    public BuildingData buildingData; // Reference to the BuildingData scriptable object

    [Header("Units Panel")]
    public GameObject unitsPanel;
    public GameObject unitButtonPrefab;
    public Transform viewPort;
    public TextMeshProUGUI panelTitleText;
    public GameObject confirmationPanel;
    public Transform viewPortCost;

    [Header("Units to Unlock")]
    public List<UnitSO> unitsToUnlock = new List<UnitSO>(); // List of units to unlock
    public List<UnitSO> meleeUnitsToUnlock = new List<UnitSO>(); // List of melee units to unlock
    public List<UnitSO> rangedUnitsToUnlock = new List<UnitSO>(); // List of ranged units to unlock
    public List<UnitSO> supportUnitsToUnlock = new List<UnitSO>(); // List of support units to unlock
    private PlayerSO playerSO; // Reference to the PlayerSO scriptable object
    private PlayerData playerData; // Reference to the PlayerData scriptable object
    private ScrollRect unitsScrollRect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unitsPanel.SetActive(false); // Ensure the units panel is initially inactive
        playerData = Object.FindFirstObjectByType<PlayerData>();
        if (playerData == null)
        {
            Debug.LogError("UnlockUnits: PlayerData was not found in scene.");
            return;
        }

        playerSO = playerData.playerSO; // Assuming PlayerData has a reference to PlayerSO

        unitsScrollRect = unitsPanel != null ? unitsPanel.GetComponentInChildren<ScrollRect>(true) : null;
        if (unitsScrollRect != null && unitsScrollRect.content == null && viewPort is RectTransform viewportRect)
        {
            unitsScrollRect.content = viewportRect;
        }

        if (meleeButton != null) meleeButton.onClick.AddListener(meleeListBtn);
        if (rangedButton != null) rangedButton.onClick.AddListener(rangedListBtn);
        if (supportButton != null) supportButton.onClick.AddListener(supportListBtn);

        updateUnitsToUnlock(unitsToUnlock);
        meleeListBtn();
        unitsPanel.SetActive(false);
    }

    public void SetBuildingData(BuildingData data)
    {
        buildingData = data;
        UpdateBuildingIcon();
    }

    private void UpdateBuildingIcon()
    {
        if (buildingIconImage != null)
        {
            if (buildingData != null && buildingData.Icon != null)
            {
                buildingIconImage.gameObject.SetActive(true);
                buildingIconImage.sprite = buildingData.Icon;
            }
            else
            {
                buildingIconImage.gameObject.SetActive(false);
            }
        }
    }

    public void updateUnitsToUnlock(List<UnitSO> newUnits)
    {
        unitsToUnlock = newUnits != null ? new List<UnitSO>(newUnits) : new List<UnitSO>();

        if (playerSO != null && playerSO.unlockedUnits != null)
        {
            unitsToUnlock.RemoveAll(unit => unit == null || playerSO.unlockedUnits.Contains(unit));
        }
        else
        {
            unitsToUnlock.RemoveAll(unit => unit == null);
        }

        meleeUnitsToUnlock.Clear();
        meleeUnitsToUnlock.AddRange(unitsToUnlock.FindAll(unit => unit.unitType == UnitSO.UnitType.Melee));
        rangedUnitsToUnlock.Clear();
        rangedUnitsToUnlock.AddRange(unitsToUnlock.FindAll(unit => unit.unitType == UnitSO.UnitType.Ranger));
        supportUnitsToUnlock.Clear();
        supportUnitsToUnlock.AddRange(unitsToUnlock.FindAll(unit => unit.unitType == UnitSO.UnitType.Support));
    }

    public void meleeListBtn()
    {
        FillUnitsPanel("Melee Units", UnitSO.UnitType.Melee);
    }

    public void rangedListBtn()
    {
        FillUnitsPanel("Ranged Units", UnitSO.UnitType.Ranger);
    }

    public void supportListBtn()
    {
        FillUnitsPanel("Support Units", UnitSO.UnitType.Support);
    }

    private void FillUnitsPanel(string title, UnitSO.UnitType unitType)
    {
        if (unitsPanel != null)
        {
            unitsPanel.SetActive(true);
        }

        if (playerSO != null)
        {
            updateUnitsToUnlock(unitsToUnlock);
        }

        if (viewPort == null || unitButtonPrefab == null)
        {
            Debug.LogError("UnlockUnits.FillUnitsPanel: Missing viewport or unit button prefab reference.");
            return;
        }

        if (panelTitleText != null)
        {
            panelTitleText.text = title;
        }

        Transform buttonParent = unitsScrollRect != null && unitsScrollRect.content != null
            ? unitsScrollRect.content
            : viewPort;

        for (int i = buttonParent.childCount - 1; i >= 0; i--)
        {
            Destroy(buttonParent.GetChild(i).gameObject);
        }

        if (unitsToUnlock == null)
        {
            Debug.LogWarning("UnlockUnits.FillUnitsPanel: unitsToUnlock is null.");
            return;
        }

        for (int i = 0; i < unitsToUnlock.Count; i++)
        {
            UnitSO unit = unitsToUnlock[i];
            if (unit == null)
            {
                continue;
            }

            if (unit.unitType != unitType)
            {
                continue;
            }

           GameObject button = Instantiate(unitButtonPrefab, buttonParent);
           UnlockUnitBtn trainButton = button.GetComponent<UnlockUnitBtn>();
           if (trainButton == null)
            {
                Debug.LogError("UnlockUnits.FillUnitsPanel: Button prefab is missing UnlockUnitBtn.");
                Destroy(button);
                continue;
            }

        trainButton.unitToUnlock = unit;
        trainButton.confirmationPanel = confirmationPanel; // pass down the shared panel
        trainButton.costParent = viewPortCost;              // pass down the shared cost parent
        trainButton.unlockTroopPanel = this.gameObject; // pass down the reference to this panel

        // set the button's own label immediately, don't wait for confirmation panel
        if (trainButton.unitNameText != null)
            trainButton.unitNameText.text = unit.unitName;
        if (trainButton.unitIconImage != null && unit.unitIcon != null)
            trainButton.unitIconImage.sprite = unit.unitIcon;
        }

        if (buttonParent.childCount == 0)
        {
            Debug.LogWarning($"UnlockUnits.FillUnitsPanel: No units matched type {unitType}.");
        }
    }

    public void exitButton()
    {
        Destroy(gameObject);
    }

    public void unlockUnit()
    {
        if (playerSO == null)
        {
            Debug.LogError("UnlockUnits.unlockUnit: PlayerSO is not assigned.");
            return;
        }

        if (selectedUnit == null)
        {
            Debug.LogWarning("UnlockUnits.unlockUnit: No unit is selected.");
            return;
        }

        if (selectedUnit.buildingData == null)
        {
            Debug.LogWarning("UnlockUnits.unlockUnit: Selected unit has no cost data.");
            return;
        }

        if (playerSO.unlockedUnits != null && playerSO.unlockedUnits.Contains(selectedUnit))
        {
            Debug.Log($"Unit {selectedUnit.unitName} is already unlocked.");
            return;
        }

        // Same multiplier used when displaying costs in UnlockUnitBtn.createCostList
        const int costMultiplier = 4;
        BuildingData cost = selectedUnit.buildingData;
        int woodCost = cost.woodCost * costMultiplier;
        int rockCost = cost.rockCost * costMultiplier;
        int farmCost = cost.farmCost * costMultiplier;
        int coinCost = cost.coinCost * costMultiplier;

        if (woodCost > playerSO.woodResources
            || rockCost > playerSO.stoneResources
            || farmCost > playerSO.farmResources
            || coinCost > playerSO.coins)
        {
            Debug.Log($"UnlockUnits.unlockUnit: Not enough resources to unlock {selectedUnit.unitName}.");
            return;
        }

        playerSO.woodResources -= woodCost;
        playerSO.stoneResources -= rockCost;
        playerSO.farmResources -= farmCost;
        playerSO.coins -= coinCost;

        if (playerData != null)
        {
            playerData.updatePlayerMaterials();
        }

        if (playerSO.unlockedUnits == null)
        {
            playerSO.unlockedUnits = new List<UnitSO>();
        }

        playerSO.unlockedUnits.Add(selectedUnit);
        Debug.Log($"Unlocked unit: {selectedUnit.unitName}");

        updateUnitsToUnlock(unitsToUnlock);

        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }

        if (unitsPanel != null && unitsPanel.activeSelf)
        {
            meleeListBtn();
        }

        selectedUnit = null;
    }

    public void closeConfirmationPanel()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }
}
