using UnityEngine;
using TMPro;
using UnityEngine.UI; // ADDED: Necessary namespace for working with the Image component

public class InfoPanel : MonoBehaviour
{
    public static InfoPanel Instance { get; private set; }
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public GameObject gameObjectParent;
    public PlayerSO playerData;
    
    public string valueTextName;

    [Header("Global UI Elements")]
    public Image troopIconImage; // ADDED: Drag the layout image placeholder here!

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
        // Cache the incoming references locally so your data fields match up perfectly!
        this.buildingStatsSO = buildingStatsSO;
        this.unitData = unitData;

        if (buildingStatsSO != null && unitData == null)
        {
            unitInfoParent.SetActive(false);
            buildingInfoParent.SetActive(true);
            
            // Hide the troop icon when viewing a structure panel layout card
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

            // UPDATED: Dynamically handle updating your unit profile image asset
            if (troopIconImage != null)
            {
                // Note: Change 'unitIcon' to whatever variable name you defined inside UnitData.cs!
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
        gameObject.SetActive(false);
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
                passiveResource.upgradeBuilding();
                SetUp(buildingStatsSO, null);
            }
            if (unitData != null)
            {
                //upgrade unit logic
                SetUp(null, unitData);
            }
        }
    }
}