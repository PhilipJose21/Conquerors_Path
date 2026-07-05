using UnityEngine;
using UnityEngine.UI;

public class BuildModeManager : MonoBehaviour
{
    [Header("Main Toggle Controls")]
    [SerializeField] private GameObject buildModePanel;
    [SerializeField] private Button buildModeToggleButton; // The main button on your Kingdom HUD

    [Header("Tab Navigation Buttons")]
    [SerializeField] private Button defensesButton;
    [SerializeField] private Button economyButton;
    [SerializeField] private Button decorativeButton;

    [Header("Sub Category Scroll Panels")]
    [SerializeField] private GameObject defensesBuildPanel;
    [SerializeField] private GameObject economyBuildPanel;
    [SerializeField] private GameObject decorativeBuildPanel;

    [Header("Tab Visual Feedback Settings")]
    [Range(0f, 1f)] 
    [SerializeField] private float inactiveAlphaOrDim = 0.4f; 

    private Color originalDefensesColor;
    private Color originalEconomyColor;
    private Color originalDecorativeColor;
    private bool colorsCached = false;

    void Start()
    {
        // 1. Setup our main UI toggle hook
        if (buildModeToggleButton != null) buildModeToggleButton.onClick.AddListener(ToggleBuildMode);

        // 2. Safely map sub-category tab click handlers
        if (defensesButton != null)   defensesButton.onClick.AddListener(OpenDefensesTab);
        if (economyButton != null)    economyButton.onClick.AddListener(OpenEconomyTab);
        if (decorativeButton != null) decorativeButton.onClick.AddListener(OpenDecorativeTab);

        // Default to showing the defenses row on startup
        OpenDefensesTab();
    }

    public void ToggleBuildMode()
    {
        if (buildModePanel == null) return;

        bool isCurrentlyActive = buildModePanel.activeSelf;
        buildModePanel.SetActive(!isCurrentlyActive);
    }

    // --- TAB TOGGLE SELECTION METHODS ---

    public void OpenDefensesTab()
    {
        if (defensesBuildPanel != null)   defensesBuildPanel.SetActive(true);
        if (economyBuildPanel != null)    economyBuildPanel.SetActive(false);
        if (decorativeBuildPanel != null) decorativeBuildPanel.SetActive(false);

        SetActiveTabVisuals(defensesButton);
    }

    public void OpenEconomyTab()
    {
        if (defensesBuildPanel != null)   defensesBuildPanel.SetActive(false);
        if (economyBuildPanel != null)    economyBuildPanel.SetActive(true);
        if (decorativeBuildPanel != null) decorativeBuildPanel.SetActive(false);

        SetActiveTabVisuals(economyButton);
    }

    public void OpenDecorativeTab()
    {
        if (defensesBuildPanel != null)   defensesBuildPanel.SetActive(false);
        if (economyBuildPanel != null)    economyBuildPanel.SetActive(false);
        if (decorativeBuildPanel != null) decorativeBuildPanel.SetActive(true);

        SetActiveTabVisuals(decorativeButton);
    }

    // --- COLOR TINT RETENTION SYSTEM ---

    private void CacheOriginalColors()
    {
        if (colorsCached) return;

        if (defensesButton != null)   originalDefensesColor = defensesButton.image.color;
        if (economyButton != null)    originalEconomyColor = economyButton.image.color;
        if (decorativeButton != null) originalDecorativeColor = decorativeButton.image.color;

        colorsCached = true;
    }

    private void SetActiveTabVisuals(Button activeBtn)
    {
        CacheOriginalColors();

        // Darken unselected categories down based on the editor slider percentage
        if (defensesButton != null)   defensesButton.image.color = originalDefensesColor * new Color(inactiveAlphaOrDim, inactiveAlphaOrDim, inactiveAlphaOrDim, 1f);
        if (economyButton != null)    economyButton.image.color = originalEconomyColor * new Color(inactiveAlphaOrDim, inactiveAlphaOrDim, inactiveAlphaOrDim, 1f);
        if (decorativeButton != null) decorativeButton.image.color = originalDecorativeColor * new Color(inactiveAlphaOrDim, inactiveAlphaOrDim, inactiveAlphaOrDim, 1f);

        // Restore vibrant active coloring to selected button asset
        if (activeBtn == defensesButton && defensesButton != null)     defensesButton.image.color = originalDefensesColor;
        if (activeBtn == economyButton && economyButton != null)       economyButton.image.color = originalEconomyColor;
        if (activeBtn == decorativeButton && decorativeButton != null) decorativeButton.image.color = originalDecorativeColor;
    }
}