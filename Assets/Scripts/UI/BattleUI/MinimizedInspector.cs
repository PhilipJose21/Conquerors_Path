using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinimizedInspector : MonoBehaviour
{
    public static MinimizedInspector Instance { get; private set; }

    [Header("Core UI Fields (Always Visible)")]
    public Image unitIconImage; 
    public GameObject toggleButton; 

    [Header("Detailed Fields (Hidden when Minimized)")]
    public TextMeshProUGUI lvlText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI healthText;
    public Image healthBarFill;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI rangeText;

    [HideInInspector] public bool isLockedOpen = false;
    private bool isExpanded = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        gameObject.SetActive(false); 
        isExpanded = false;
        isLockedOpen = false;
    }

    public void ShowUnitStats(UnitSO unitData, int currentHP, int maxHP)
    {
        if (unitData == null) return;
        Debug.Log($"[UI Update] Unit Name: '{unitData.unitName}', Atk: {unitData.damage}, Icon Sprite Name: {unitData.unitIcon?.name}");
        gameObject.SetActive(true);
        isLockedOpen = true;

        if (unitIconImage != null) unitIconImage.sprite = unitData.unitIcon;
        if (nameText != null) nameText.text = unitData.unitName;
        if (healthText != null) healthText.text = $"Health: {currentHP} / {maxHP}";
        if (healthBarFill != null) healthBarFill.fillAmount = (float)currentHP / maxHP;
        // if (lvlText != null) lvlText.text = $"Lvl: {unitData.level}";
        if (attackText != null) attackText.text = $"Atk: {unitData.damage}";
        if (rangeText != null) rangeText.text = $"Range: {unitData.attackRange}x{unitData.attackRange}";

        SetExpandedState(false);
    }

    public void ToggleExpand()
    {
        SetExpandedState(!isExpanded);
    }

    public void SetExpandedState(bool expand)
    {
        isExpanded = expand;

        // 🌟 ONLY toggle the stats sub-elements, do NOT touch the main panel or the exit button
        if (nameText != null) nameText.gameObject.SetActive(isExpanded);
        if (healthText != null) healthText.gameObject.SetActive(isExpanded);
        if (attackText != null) attackText.gameObject.SetActive(isExpanded);
        if (rangeText != null) rangeText.gameObject.SetActive(isExpanded);
        // if (lvlText != null) lvlText.gameObject.SetActive(isExpanded);

        if (healthBarFill != null && healthBarFill.transform.parent != null)
        {
            healthBarFill.transform.parent.gameObject.SetActive(isExpanded);
        }
    }

    // ❌ Called ONLY by your dedicated Red X Exit Button
    public void CloseInspector()
    {
        isLockedOpen = false; // Release the lock
        isExpanded = false;
        gameObject.SetActive(false); // Cleanly turn off the panel
    }
}