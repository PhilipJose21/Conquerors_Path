using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class BuildModeCard : MonoBehaviour
{
    [Header("Building Blueprint Source")]
    [SerializeField] private BuildingData placementData; 

    [Header("UI Component Target Slots")]
    [SerializeField] private Image buildingIconImage;    
    [SerializeField] private TextMeshProUGUI titleText;    

    private BuildingSystem buildingSystem;
    private Button wholeCardButton;

    void Start()
    {
        buildingSystem = Object.FindFirstObjectByType<BuildingSystem>();
        wholeCardButton = GetComponent<Button>();

        if (placementData != null)
        {
            if (titleText != null) 
            {
                titleText.text = placementData.Name;
            }

            if (buildingIconImage != null && placementData.Icon != null)
            {
                buildingIconImage.sprite = placementData.Icon;
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
            if (buildingSystem.SelectBuildingByData(placementData))
            {
                KingdomUIManager.Instance?.ToggleBuildMode();
            }
        }
    }
}