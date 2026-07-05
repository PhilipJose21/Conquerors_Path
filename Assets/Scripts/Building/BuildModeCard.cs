using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class BuildModeCard : MonoBehaviour
{
    [Header("GDD Scriptable Object Links")]
    [SerializeField] private BuildingData placementData;      // Handles Grid Size & Cost for BuildingSystem
    [SerializeField] private BuildingStatsSO productionStats; // Handles Names, Descriptions, & Timers

    [Header("UI Component Target Slots")]
    [SerializeField] private Image buildingIconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI gridSizeText;
    [SerializeField] private TextMeshProUGUI costText;

    private BuildingSystem buildingSystem;
    private Button wholeCardButton;

    void Start()
    {
        buildingSystem = Object.FindFirstObjectByType<BuildingSystem>();
        wholeCardButton = GetComponent<Button>();

        // 1. Pull Identity info from BuildingStatsSO
        if (productionStats != null)
        {
            if (titleText != null) titleText.text = productionStats.buildingName;
            if (descriptionText != null) descriptionText.text = productionStats.description;
        }

        // 2. Pull Cost, Size, and Icons from BuildingData
        if (placementData != null)
        {
            if (buildingIconImage != null && placementData.Icon != null)
            {
                buildingIconImage.sprite = placementData.Icon;
            }

            if (gridSizeText != null)
            {
                // Explicitly rendering layout boundaries using the size integer
                gridSizeText.text = $"Size: {placementData.Size}x{placementData.Size}";
            }

            if (costText != null)
            {
                string costString = "";
                if (placementData.coinCost > 0) costString += $"{placementData.coinCost} Gold  ";
                if (placementData.woodCost > 0) costString += $"{placementData.woodCost} Wood  ";
                if (placementData.rockCost > 0) costString += $"{placementData.rockCost} Stone  ";
                if (placementData.farmCost > 0) costString += $"{placementData.farmCost} Farm  ";
                
                costText.text = string.IsNullOrEmpty(costString) ? "Free to Build" : costString.TrimEnd();
            }
        }

        if (wholeCardButton != null)
        {
            wholeCardButton.onClick.AddListener(OnCardClicked);
        }
    }

    private void OnCardClicked()
    {
        if (placementData == null) return;

        if (buildingSystem != null)
        {
            // Successfully passes the exact scriptable object type your grid architecture demands!
            buildingSystem.SelectBuildingByData(placementData);
            KingdomUIManager.Instance?.ToggleBuildMode();
        }
    }
}