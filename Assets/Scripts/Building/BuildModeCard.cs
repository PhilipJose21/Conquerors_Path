using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class BuildModeCard : MonoBehaviour
{
    [Header("Building Data Source of Truth")]
    [SerializeField] private BuildingData buildingConfigData; 

    [Header("UI Component Target Slots")]
    [SerializeField] private Image buildingIconImage;
    [SerializeField] private TextMeshProUGUI countOrLevelText; 

    private BuildingSystem buildingSystem;
    private BuildModeManager buildModeManager;
    private Button wholeCardButton;

    void Start()
    {
        buildingSystem = Object.FindFirstObjectByType<BuildingSystem>();
        buildModeManager = Object.FindFirstObjectByType<BuildModeManager>();
        wholeCardButton = GetComponent<Button>();

        if (buildingConfigData != null)
        {
            if (buildingIconImage != null && buildingConfigData.Icon != null)
            {
                buildingIconImage.sprite = buildingConfigData.Icon;
            }

            if (countOrLevelText != null)
            {
                countOrLevelText.text = $"{buildingConfigData.Size}x{buildingConfigData.Size}";
            }
        }

        if (wholeCardButton != null)
        {
            wholeCardButton.onClick.AddListener(OnCardClicked);
        }
    }

    private void OnCardClicked()
    {
        if (buildingConfigData == null) return;

        if (buildingSystem != null)
        {
            buildingSystem.SelectBuildingByData(buildingConfigData);
            
            if (buildModeManager != null)
            {
                buildModeManager.ToggleBuildMode();
            }
        }
    }
}