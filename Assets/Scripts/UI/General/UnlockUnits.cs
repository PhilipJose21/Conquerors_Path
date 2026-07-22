using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class UnlockUnits : MonoBehaviour
{
    public Button meleeButton; // Button for unlocking melee units
    public Button rangedButton; // Button for unlocking ranged units
    public Button supportButton; // Button for unlocking support units

    [Header("Units Panel")]
    public GameObject unitsPanel;
    public GameObject unitButtonPrefab;
    public Transform viewPort;
    public TextMeshProUGUI panelTitleText;

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
            TrainTroopsButton trainButton = button.GetComponent<TrainTroopsButton>();
            if (trainButton == null)
            {
                Debug.LogError("UnlockUnits.FillUnitsPanel: Button prefab is missing TrainTroopsButton.");
                Destroy(button);
                continue;
            }

            trainButton.unitToTrain = unit;
            trainButton.isTraining = false;
            trainButton.isUpgrading = false;
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
}
