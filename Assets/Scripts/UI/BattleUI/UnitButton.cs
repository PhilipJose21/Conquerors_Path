using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitButton : MonoBehaviour
{
    public BuildingData buildingData;
    public UnitSO unitData;
    public Image unitIconRenderer;
    public TextMeshProUGUI unitCostText, unitAmountText;
    public GameObject fadedOverlay;
    private string unitName;

    public int unitCount = 1;
    
    void Start()
    {
        if (unitData != null)
        {
            if (unitData.unitIcon != null)
            {
                unitIconRenderer.sprite = unitData.unitIcon;
            }
            unitName = unitData.unitName;
            unitCostText.text = buildingData.reinforcementCost.ToString();
            unitAmountText.text = unitCount.ToString(); //amount of this unit is in inventory
        }
    }

    void Update()
    {
        unitAmountText.text = unitCount.ToString();
        if (unitCount <= 0)
        {
            fadedOverlay.SetActive(true);
        }
        else
        {
            fadedOverlay.SetActive(false);
        }
    }

    public void ButtonClicked()
    {
        Debug.Log(unitName);

        BuildingSystem buildingSystem = Object.FindFirstObjectByType<BuildingSystem>();
        if (buildingSystem != null)
        {

            //only allows the button selection if its inside the battle scene
            if (buildingSystem.isBattleScene)
            {
                buildingSystem.SelectBuildingByData(buildingData);
            }
            else
            {
                Debug.LogWarning("NOT IN BATTLE SCENE");
            }
        }
        else{
            Debug.LogError("NO BUILDING SYSTEM FOUND IN SCENE");
        }
    }
}
