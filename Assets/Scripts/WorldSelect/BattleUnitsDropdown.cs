using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class BattleUnitsDropdown : MonoBehaviour
{
    [Header("UI Panel Toggles")]
    [SerializeField] private GameObject unitsContentPanel; // The actual dropdown menu box (SystemMenuPanel)
    [SerializeField] private Button toggleButton;           // The main button players click

    [Header("Prefab Layout Templates")]
    [SerializeField] private GameObject unitDisplayPrefab;   
    [SerializeField] private Transform gridParent;           

    [Header("Dropdown Animation Settings")]
    [SerializeField] private float animationDuration = 0.2f; // Time in seconds for the animation

    private PlayerData playerData;
    private PlayerBattleSO playerBattleSO;
    private bool isDropdownOpen = false;
    private Coroutine toggleCoroutine;
    private RectTransform panelRectTransform;

    private void Start()
    {
        playerData = Object.FindFirstObjectByType<PlayerData>();
        if (playerData == null)
        {
            Debug.LogError("BattleUnitsDropdown: PlayerData was not found in the scene.");
            return;
        }

        playerBattleSO = playerData.playerBattleSO;

        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleDropdown);
        }

        if (unitsContentPanel != null)
        {
            panelRectTransform = unitsContentPanel.GetComponent<RectTransform>();
            
            // Set the pivot to the TOP center so it scales downwards like a dropdown window
            if (panelRectTransform != null)
            {
                panelRectTransform.pivot = new Vector2(0.5f, 1f);
            }
            
            unitsContentPanel.SetActive(false);
        }
    }

    public void ToggleDropdown()
    {
        if (unitsContentPanel == null) return;

        isDropdownOpen = !isDropdownOpen;

        // If an animation is already running, stop it first to prevent stuttering
        if (toggleCoroutine != null)
        {
            StopCoroutine(toggleCoroutine);
        }

        // Start the smooth dropdown/rollup animation
        toggleCoroutine = StartCoroutine(AnimateDropdown(isDropdownOpen));
    }

    private IEnumerator AnimateDropdown(bool open)
    {
        float elapsed = 0f;
        Vector3 startScale = panelRectTransform.localScale;
        Vector3 targetScale = open ? Vector3.one : new Vector3(1f, 0f, 1f);

        if (open)
        {
            // Populate data right before showing the panel
            RefreshRosterDisplay();
            unitsContentPanel.SetActive(true);
            // Start completely flat vertically
            startScale = new Vector3(1f, 0f, 1f);
            panelRectTransform.localScale = startScale;
        }

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            
            // Smooth deceleration curve
            t = Mathf.SmoothStep(0f, 1f, t);

            // Interpolate the vertical Y-scale smoothly
            panelRectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        // Ensure perfect final values snap
        panelRectTransform.localScale = targetScale;

        if (!open)
        {
            unitsContentPanel.SetActive(false);
        }

        toggleCoroutine = null;
    }

    private void RefreshRosterDisplay()
    {
        if (gridParent == null || unitDisplayPrefab == null || playerBattleSO == null) return;

        for (int i = gridParent.childCount - 1; i >= 0; i--)
        {
            Destroy(gridParent.GetChild(i).gameObject);
        }

        List<UnitSO> activeTroops = playerBattleSO.playerUnitStats; //
        if (activeTroops == null || activeTroops.Count == 0)
        {
            Debug.LogWarning("BattleUnitsDropdown: No active battle units found in PlayerBattleSO.");
            return;
        }

        Dictionary<string, (UnitSO unitData, int count)> groupedTroops = new Dictionary<string, (UnitSO, int)>();

        foreach (UnitSO unit in activeTroops)
        {
            if (unit == null) continue;

            if (groupedTroops.ContainsKey(unit.unitName))
            {
                var existing = groupedTroops[unit.unitName];
                groupedTroops[unit.unitName] = (existing.unitData, existing.count + 1);
            }
            else
            {
                groupedTroops[unit.unitName] = (unit, 1);
            }
        }

        foreach (var entry in groupedTroops.Values)
        {
            GameObject displayInstance = Instantiate(unitDisplayPrefab, gridParent);
            Image iconImage = displayInstance.GetComponentInChildren<Image>();
            TextMeshProUGUI countText = displayInstance.GetComponentInChildren<TextMeshProUGUI>();

            if (countText != null)
            {
                countText.text = $"x{entry.count}";
            }

            if (iconImage != null && entry.unitData.unitIcon != null)
            {
                iconImage.sprite = entry.unitData.unitIcon; //
            }
        }
    }
}