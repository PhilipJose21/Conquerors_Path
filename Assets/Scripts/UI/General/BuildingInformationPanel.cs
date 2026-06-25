using UnityEngine;
using TMPro;

public class BuildingInformationPanel : MonoBehaviour
{
    
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;

    [Header("Building UI Elements")]
    public GameObject buildingInfoParent;
    public GameObject resourceOutputParent;
    public GameObject resourceTypeParent;


    public TextMeshProUGUI resourceTypeText;
    public TextMeshProUGUI resourceAmountText;


    [Header("Unit UI Elements")]
    public GameObject unitInfoParent;

    public GameObject unitTypeParent;
    public GameObject hpParent;
    public GameObject damageParent;
    public GameObject attackRangeParent;
    public GameObject mobilityParent;
    public GameObject unitCostParent;

    public TextMeshProUGUI unitTypeText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI attackRangeText;
    public TextMeshProUGUI mobilityText;
    public TextMeshProUGUI unitCostText;

    public string valueTextName;

    public BuildingStatsSO buildingData;
    public TroopData unitData;

    void Awake()
    {
        resourceTypeText = resourceTypeParent.transform.Find(valueTextName)?.GetComponent<TextMeshProUGUI>();
        resourceAmountText = resourceOutputParent.transform.Find(valueTextName)?.GetComponent<TextMeshProUGUI>();
        unitTypeText = unitTypeParent.transform.Find(valueTextName)?.GetComponent<TextMeshProUGUI>();
        hpText = hpParent.transform.Find(valueTextName)?.GetComponent<TextMeshProUGUI>();
        damageText = damageParent.transform.Find(valueTextName)?.GetComponent<TextMeshProUGUI>();
        attackRangeText = attackRangeParent.transform.Find(valueTextName)?.GetComponent<TextMeshProUGUI>();
        mobilityText = mobilityParent.transform.Find(valueTextName)?.GetComponent<TextMeshProUGUI>();
        unitCostText = unitCostParent.transform.Find(valueTextName)?.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (buildingData != null)
        {
            SetUp(buildingData, null);
        }
        else if (unitData != null)
        {
            SetUp(null, unitData);
        }
    }

    public void SetUp(BuildingStatsSO buildingData, TroopData unitData)
    {
        if (buildingData != null && unitData == null)
        {
            unitInfoParent.SetActive(false);
            buildingInfoParent.SetActive(true);
            nameText.text = buildingData.buildingName;
            descriptionText.text = buildingData.description;
            resourceTypeText.text = buildingData.resourceType.ToString();
            resourceAmountText.text = (buildingData.resourceAmount.ToString() + " %");
        }

        if (unitData != null && buildingData == null)
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
        }

        
    }
}
