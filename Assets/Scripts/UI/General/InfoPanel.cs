using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour
{
    public static InfoPanel Instance { get; private set; }
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public GameObject gameObjectParent;
    public PlayerSO playerData;
    public Button upgradeButtonUI;
    public TextMeshProUGUI upgradeButtonText;
    
    public string valueTextName;

    [Header("Global UI Elements")]
    public Image troopIconImage;

    [Header("Building UI Elements")]
    public GameObject buildingInfoParent;
    public GameObject resourceOutputParent;
    public GameObject resourceTypeParent;
    public Image buildingIconImage; // Drag your Building Icon Image component here in the Inspector!

    private TextMeshProUGUI resourceTypeText;
    private TextMeshProUGUI resourceAmountText;

    [Header("Unit UI Elements")]
    public GameObject unitInfoParent;
    public GameObject unitTypeParent;
    public GameObject hpParent;
    public GameObject damageParent;
    public GameObject attackRangeParent;
    public GameObject mobilityParent;
    public GameObject unitCostParent;

    private TextMeshProUGUI unitTypeText;
    private TextMeshProUGUI hpText;
    private TextMeshProUGUI damageText;
    private TextMeshProUGUI attackRangeText;
    private TextMeshProUGUI mobilityText;
    private TextMeshProUGUI unitCostText;

    [Header("Upgrade Confirmation UI")]
    public GameObject confirmationPanel;
    public TextMeshProUGUI costTextDisplay;

    public BuildingStatsSO buildingStatsSO;
    public BuildingData buildingData;
    public UnitSO unitData;

    private PassiveResource passiveResource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"InfoPanel: duplicate instance detected on '{gameObject.name}'. " +
                $"Keeping existing Instance on '{Instance.gameObject.name}' and leaving this one alone.", this);
            return;
        }
        Instance = this;

        playerData = Object.FindFirstObjectByType<PlayerData>().playerSO;

        resourceTypeText = resourceTypeParent.transform.Find(valueTextName)?.GetComponent<TextMeshProUGUI>();
        resourceAmountText = resourceOutputParent.transform.Find(valueTextName)?.GetComponent<TextMeshProUGUI>();
        unitTypeText = unitTypeParent.transform.Find(valueTextName)?.GetComponent<TextMeshProUGUI>();
        hpText = hpParent.transform.Find(valueTextName)?.GetComponent<TextMeshProUGUI>();
        damageText = damageParent.transform.Find(valueTextName)?.GetComponent<TextMeshProUGUI>();
        attackRangeText = attackRangeParent.transform.Find(valueTextName)?.GetComponent<TextMeshProUGUI>();
        mobilityText = mobilityParent.transform.Find(valueTextName)?.GetComponent<TextMeshProUGUI>();
        unitCostText = unitCostParent.transform.Find(valueTextName)?.GetComponent<TextMeshProUGUI>();

        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }

    void Start()
    {
        if (gameObjectParent != null)
        {
            passiveResource = gameObjectParent.GetComponentInChildren<PassiveResource>();
        }
    }

    void Update()
    {
        // 🚨 Frame-by-frame Setup logic removed to prevent UI resetting/flickering.
        // KingdomUIManager handles this smoothly on-click.
    }

    public void SetUp(BuildingStatsSO buildingStatsSO, UnitSO unitData)
    {
        this.buildingStatsSO = buildingStatsSO;
        this.unitData = unitData;

        // --- 1. HANDLING BUILDING SELECTION ---
        if (buildingStatsSO != null && unitData == null)
        {
            if (unitInfoParent != null) unitInfoParent.SetActive(false);
            if (buildingInfoParent != null) buildingInfoParent.SetActive(true);
            
            if (troopIconImage != null) troopIconImage.gameObject.SetActive(false);

            if (nameText != null) nameText.text = buildingStatsSO.buildingName;
            if (descriptionText != null) descriptionText.text = buildingStatsSO.description;
            if (resourceTypeText != null) resourceTypeText.text = buildingStatsSO.resourceType.ToString();
            
            // Set the building icon sprite using buildingData.Icon
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
            
            if (gameObjectParent != null)
            {
                passiveResource = gameObjectParent.GetComponentInChildren<PassiveResource>();
            }
            
            if (passiveResource != null && resourceAmountText != null)
            {
                resourceAmountText.text = passiveResource.resourceAmount.ToString() + " %";

                // Check if the resource has reached the 100% cap
                if (passiveResource.resourceAmount >= 100)
                {
                    if (upgradeButtonUI != null) upgradeButtonUI.interactable = false;
                    if (upgradeButtonText != null) upgradeButtonText.text = "MAX LEVEL";
                }
                else
                {
                    if (upgradeButtonUI != null) upgradeButtonUI.interactable = true;
                    if (upgradeButtonText != null) upgradeButtonText.text = "UPGRADE";
                }
            }
        }
        // --- 2. ⚔️ HANDLING TROOP SELECTION ---
        else if (unitData != null)
        {
            // Toggle view panels
            if (buildingInfoParent != null) buildingInfoParent.SetActive(false);
            if (unitInfoParent != null) unitInfoParent.SetActive(true);
            
            if (buildingIconImage != null) buildingIconImage.gameObject.SetActive(false);

            // Populate Main Header Text
            if (nameText != null) nameText.text = unitData.unitName;
            if (descriptionText != null) descriptionText.text = unitData.description;

            // Handle the Troop Sprite Icon
            if (troopIconImage != null)
            {
                if (unitData.unitIcon != null)
                {
                    troopIconImage.gameObject.SetActive(true);
                    troopIconImage.sprite = unitData.unitIcon;
                }
                else
                {
                    troopIconImage.gameObject.SetActive(false);
                }
            }

            // Populate unit-specific text attributes
            if (unitTypeText != null) unitTypeText.text = unitData.unitType.ToString();
            if (hpText != null) hpText.text = unitData.health.ToString();
            if (damageText != null) damageText.text = unitData.damage.ToString();
            if (attackRangeText != null) attackRangeText.text = unitData.attackRange.ToString();
            if (mobilityText != null) mobilityText.text = unitData.mobility.ToString();
            if (unitCostText != null) unitCostText.text = unitData.unitCost.ToString();

            // Hide the building upgrade button since this is a combat unit display
            if (upgradeButtonUI != null) upgradeButtonUI.gameObject.SetActive(false);
        }
    }

    public void closeInfoPanel()
    {
        this.gameObject.SetActive(false);
    }

    public void closeConfirmationPanel()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }

    public void destroyObject()
    {
        if (gameObjectParent != null)
        {
            passiveResource?.refundStats();
            Destroy(gameObjectParent);
        }
        gameObject.SetActive(false);
    }

    public void showConfirmation()
    {
        if (passiveResource != null)
        {
            if (buildingStatsSO != null)
            {
                if (confirmationPanel != null)
                {
                    string details = ""; 
                    if (passiveResource.coinCost > 0) details += $"Coins: {passiveResource.coinCost}\n";
                    if (passiveResource.farmCost > 0) details += $"Food: {passiveResource.farmCost}\n";
                    if (passiveResource.rockCost > 0) details += $"Rock: {passiveResource.rockCost}\n";
                    if (passiveResource.woodCost > 0) details += $"Wood: {passiveResource.woodCost}\n";
                    if (passiveResource.gemCost > 0) details += $"Gems: {passiveResource.gemCost}\n";
                    if (passiveResource.energyCost > 0) details += $"Energy: {passiveResource.energyCost}\n";

                    if (costTextDisplay != null) costTextDisplay.text = details;
                    confirmationPanel.SetActive(true);
                }
                else
                {
                    ConfirmUpgrade();
                }
            }
            if (unitData != null)
            {
                SetUp(null, unitData);
            }
        }
    }

    public void ConfirmUpgrade()
    {
        if (passiveResource != null && buildingStatsSO != null)
        {
            passiveResource.upgradeBuilding();
            if (confirmationPanel != null) confirmationPanel.SetActive(false);
            SetUp(buildingStatsSO, null);
        }
    }

    public void CancelUpgrade()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }
}