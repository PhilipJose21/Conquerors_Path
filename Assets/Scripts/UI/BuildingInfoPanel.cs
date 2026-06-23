using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuildingInfoPanel : MonoBehaviour
{
    // Common UI fields
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;

    // Stats grid (Now arranged in a single flat vertical list)
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI mobilityText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI resourceText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI unitCostText;
    public TextMeshProUGUI attackRangeText;
    public TextMeshProUGUI upgradeCostText;

    // Buttons
    public Button upgradeButton;
    public Button destroyRetireButton;
    public Button closeButton;

    void Awake()
    {
        if (closeButton == null)
        {
            var closeTransform = transform.Find("ExitButton") ?? transform.Find("CloseButton");
            if (closeTransform != null)
            {
                closeButton = closeTransform.GetComponent<Button>();
            }
        }
        if (closeButton != null)
            closeButton.onClick.AddListener(OnClosePressed);
        else
            Debug.LogWarning("BuildingInfoPanel: closeButton is not assigned and no ExitButton/CloseButton child was found.");
    }

    // Setup from an instantiated Building instance
    public void Setup(Building building)
    {
        if (building == null) return;
        Setup(InfoPanelViewData.ForBuilding(building));
    }

    // Setup from BuildingData asset (e.g., when selecting from UI list)
    public void Setup(BuildingData data)
    {
        if (data == null) return;
        Setup(InfoPanelViewData.ForBuilding(data));
    }

    // Support troops via a simple TroopData ScriptableObject
    public void Setup(TroopData troop)
    {
        if (troop == null) return;
        Setup(InfoPanelViewData.ForTroop(troop));
    }

    // Generic setup for any object type represented by InfoPanelViewData.
    public void Setup(InfoPanelViewData viewData)
    {
        if (viewData == null) return;

        PopulateCommon(viewData.title, viewData.icon, viewData.description);

        UpdateStatField(typeText, "Type", viewData.type);
        UpdateStatField(mobilityText, "Mobility", viewData.mobility);
        UpdateStatField(hpText, "HP", viewData.hp);
        UpdateStatField(resourceText, "Resources", viewData.resource);
        UpdateStatField(damageText, "Damage", viewData.damage);
        UpdateStatField(unitCostText, "Cost", viewData.unitCost);
        UpdateStatField(attackRangeText, "Attack Range", viewData.attackRange);
        UpdateStatField(upgradeCostText, "Upgrade Cost", viewData.upgradeCost);

        ShowUpgradeAndDestroy(viewData.showUpgrade, viewData.showDestroy);
    }

    private void PopulateCommon(string name, Sprite icon, string description)
    {
        if (nameText != null) nameText.text = name ?? "";
        if (iconImage != null) iconImage.sprite = icon;
        
        if (descriptionText != null)
        {
            descriptionText.text = description ?? "";
            descriptionText.gameObject.SetActive(!string.IsNullOrWhiteSpace(description));
        }
    }

    private void UpdateStatField(TextMeshProUGUI textField, string label, string value)
    {
        if (textField == null) return;

        if (string.IsNullOrWhiteSpace(value))
        {
            // Instantly shuts off the GameObject so the Vertical Layout Group collapses the space!
            textField.gameObject.SetActive(false);
        }
        else
        {
            textField.text = label == "Resources" ? value : $"{label}: {value}";
            textField.gameObject.SetActive(true);
        }
    }

    private void ShowUpgradeAndDestroy(bool showBoth)
    {
        ShowUpgradeAndDestroy(showBoth, showBoth);
    }

    private void ShowUpgradeAndDestroy(bool showUpgrade, bool showDestroy)
    {
        if (upgradeButton != null) upgradeButton.gameObject.SetActive(showUpgrade);
        if (upgradeCostText != null) upgradeCostText.gameObject.SetActive(showUpgrade);
        if (destroyRetireButton != null) destroyRetireButton.gameObject.SetActive(showDestroy);
    }

    private void OnClosePressed()
    {
        KingdomUIManager.Instance?.CloseObjectInfo();
    }
}

[System.Serializable]
public class InfoPanelViewData
{
    public string title;
    public Sprite icon;
    public string description;
    public string type;
    public string mobility;
    public string hp;
    public string resource;
    public string damage;
    public string unitCost;
    public string attackRange;
    public string upgradeCost;
    public bool showUpgrade;
    public bool showDestroy;

    public static InfoPanelViewData ForBuilding(Building building)
    {
        return new InfoPanelViewData
        {
            title = building != null ? building.Name : string.Empty,
            icon = building != null ? building.Icon : null,
            description = string.Empty,
            resource = building != null ? $"Resources:\n• Wood: {building.WoodCost}\n• Stone: {building.RockCost}\n• Farm: {building.FarmCost}" : string.Empty,
            unitCost = building != null ? building.CoinCost.ToString() : string.Empty,
            upgradeCost = building != null ? building.EnergyCost.ToString() : string.Empty,
            showUpgrade = true,
            showDestroy = true
        };
    }

    public static InfoPanelViewData ForBuilding(BuildingData data)
    {
        return new InfoPanelViewData
        {
            title = data != null ? data.Name : string.Empty,
            icon = data != null ? data.Icon : null,
            description = string.Empty,
            resource = data != null ? $"Resources:\n• Wood: {data.woodCost}\n• Stone: {data.rockCost}\n• Farm: {data.farmCost}" : string.Empty,
            unitCost = data != null ? data.coinCost.ToString() : string.Empty,
            upgradeCost = data != null ? data.energyCost.ToString() : string.Empty,
            showUpgrade = true,
            showDestroy = true
        };
    }

    public static InfoPanelViewData ForTroop(TroopData troop)
    {
        int rangeSize = troop != null ? troop.attackRange * 2 + 1 : 0;
        return new InfoPanelViewData
        {
            title = troop != null ? troop.unitName : string.Empty,
            icon = troop != null ? troop.unitIcon : null,
            description = troop != null ? troop.description : string.Empty,
            type = troop != null ? troop.unitType.ToString() : string.Empty,
            mobility = troop != null ? troop.mobility.ToString() : string.Empty,
            hp = troop != null ? troop.health.ToString() : string.Empty,
            damage = troop != null ? troop.damage.ToString() : string.Empty,
            unitCost = troop != null ? troop.unitCost.ToString() : string.Empty,
            attackRange = troop != null ? rangeSize + "x" + rangeSize : string.Empty,
            showUpgrade = troop != null && troop.canUpgrade,
            showDestroy = troop != null && troop.canDestroy
        };
    }
}