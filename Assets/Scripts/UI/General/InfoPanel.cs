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
    
    public string valueTextName;

    [Header("Global UI Elements")]
    public Image troopIconImage;

    [Header("Building UI Elements")]
    public GameObject buildingInfoParent;
    public GameObject resourceOutputParent;
    public GameObject resourceTypeParent;

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
    public TroopData unitData;

    private PassiveResource passiveResource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
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
        if (buildingStatsSO != null)
        {
            SetUp(buildingStatsSO, null);
        }
        else if (unitData != null)
        {
            SetUp(null, unitData);
        }
    }

    public void SetUp(BuildingStatsSO buildingStatsSO, TroopData unitData)
    {
        this.buildingStatsSO = buildingStatsSO;
        this.unitData = unitData;

        if (buildingStatsSO != null && unitData == null)
        {
            unitInfoParent.SetActive(false);
            buildingInfoParent.SetActive(true);
            
            if (troopIconImage != null) troopIconImage.gameObject.SetActive(false);

            nameText.text = buildingStatsSO.buildingName;
            descriptionText.text = buildingStatsSO.description;
            resourceTypeText.text = buildingStatsSO.resourceType.ToString();
            
            if (passiveResource != null && resourceAmountText != null)
            {
                resourceAmountText.text = passiveResource.resourceAmount.ToString() + " %";
            }
        }

        if (unitData != null && buildingStatsSO == null)
        {
            buildingInfoParent.SetActive(false);
            unitInfoParent.SetActive(true);
            
            nameText.text = unitData.unitName;
            descriptionText.text = unitData.description;
            unitTypeText.text = unitData.unitType.ToString();
            hpText.text = unitData.health.ToString();
            damageText.text = unitData.damage.ToString();
            attackRangeText.text = unitData.attackRange.ToString();
            mobilityText.text = unitData.mobility.ToString();
            unitCostText.text = unitData.unitCost.ToString();

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
        }
    }

    public void closePanel()
    {
        Destroy(gameObject);
    }

    public void destroyObject()
    {
        if (gameObjectParent != null)
        {
            passiveResource.refundStats();
            Destroy(gameObjectParent);
        }
        gameObject.SetActive(false);
    }

    public void upgradeButton()
    {
        if (passiveResource != null)
        {
            if (buildingStatsSO != null)
            {
                if (confirmationPanel != null)
                {
                    string details = "Upgrade Cost:\n";
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