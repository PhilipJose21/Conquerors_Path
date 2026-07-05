using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class KingdomUIManager : MonoBehaviour
{
    public static KingdomUIManager Instance { get; private set; }
    
    [Header("Player Data")]
    public PlayerSO playerSO;

    [Header("Resource Text Fields")]
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI stoneText;
    public TextMeshProUGUI farmText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI researchText;
    public TextMeshProUGUI gemsText;
    public TextMeshProUGUI coinsText;

    [Header("Object Info / Building Panel")]
    public Transform objectInfoParent;
    public GameObject buildingInfoPrefab;
    [SerializeField] private GameObject currentObjectInfoPanel;
    [SerializeField] private bool currentObjectInfoPanelIsInstantiated;
    
    [Header("Troop Selection Panel")]
    public GameObject troopSelectionPanel;
    
    [Header("Bag Panel")]
    public GameObject bagPanel;

    [Header("Build Mode Panel Components")]
    [SerializeField] private GameObject buildModePanel;
    [SerializeField] private Button buildModeToggleButton; // The main Build Mode HUD Button

    [Header("Build Mode Category Buttons")]
    [SerializeField] private Button farmsButton;
    [SerializeField] private Button unitTrainingButton;
    [SerializeField] private Button miscButton;

    [Header("Build Mode Sub Category Scroll Panels")]
    [SerializeField] private GameObject farmsBuildPanel;
    [SerializeField] private GameObject unitTrainingBuildPanel;
    [SerializeField] private GameObject miscBuildPanel;

    [Header("Build Mode Visual Settings")]
    [Range(0f, 1f)] 
    [SerializeField] private float inactiveAlphaOrDim = 0.4f; 

    private Color originalFarmsColor;
    private Color originalUnitTrainingColor;
    private Color originalMiscColor;
    private bool colorsCached = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (objectInfoParent == null)
        {
            var go = GameObject.FindWithTag("ObjectInformationParent");
            if (go != null) objectInfoParent = go.transform;
        }

        if (buildingInfoPrefab == null)
        {
            buildingInfoPrefab = Resources.Load<GameObject>("UI/BuildingInfoPanel");
        }

        if (currentObjectInfoPanel == null && objectInfoParent != null)
        {
            var existingPanel = objectInfoParent.GetComponentInChildren<BuildingInfoPanel>(true);
            if (existingPanel != null)
            {
                currentObjectInfoPanel = existingPanel.gameObject;
                currentObjectInfoPanelIsInstantiated = false;
                currentObjectInfoPanel.SetActive(false);
            }
        }

        if (woodText == null) woodText = FindTMP("Canvas/ResourcePanel/WoodText");
        if (stoneText == null) stoneText = FindTMP("Canvas/ResourcePanel/StoneText");
        if (farmText == null) farmText = FindTMP("Canvas/ResourcePanel/FarmText");
        if (energyText == null) energyText = FindTMP("Canvas/ResourcePanel/EnergyText");
        if (researchText == null) researchText = FindTMP("Canvas/ResourcePanel/ResearchText");
        if (gemsText == null) gemsText = FindTMP("Canvas/ResourcePanel/GemsText");
        if (coinsText == null) coinsText = FindTMP("Canvas/ResourcePanel/CoinsText");

        playerSO = Object.FindFirstObjectByType<PlayerData>()?.playerSO;
    }

    void Start()
    {
        // Setup Build Mode Event Listeners natively
        if (buildModeToggleButton != null) buildModeToggleButton.onClick.AddListener(ToggleBuildMode);
        
        if (farmsButton != null)   farmsButton.onClick.AddListener(OpenFarmsTab);
        if (unitTrainingButton != null)    unitTrainingButton.onClick.AddListener(OpenUnitTrainingTab);
        if (miscButton != null) miscButton.onClick.AddListener(OpenMiscTab);

        // Render resource panels on load and open Farms by default
        if (playerSO != null) ShowResourceValues(playerSO);
        OpenFarmsTab();
    }

    void Update()
    {
        if (playerSO != null)
        {
            if (researchText != null) researchText.text = playerSO.researchPoints.ToString();
            if (energyText != null) energyText.text = playerSO.energyPoints.ToString();
        }
    }

    private TextMeshProUGUI FindTMP(string path)
    {
        var go = GameObject.Find(path);
        return go != null ? go.GetComponent<TextMeshProUGUI>() : null;
    }

    // --- BUILD MODE TOGGLE & CATEGORY SELECTION ---

    public void ToggleBuildMode()
    {
        if (buildModePanel == null) return;

        bool isCurrentlyActive = buildModePanel.activeSelf;
        buildModePanel.SetActive(!isCurrentlyActive);
    }

    public void OpenFarmsTab()
    {
        if (farmsBuildPanel != null)   farmsBuildPanel.SetActive(true);
        if (unitTrainingBuildPanel != null)    unitTrainingBuildPanel.SetActive(false);
        if (miscBuildPanel != null) miscBuildPanel.SetActive(false);

        SetActiveTabVisuals(farmsButton);
    }

    public void OpenUnitTrainingTab()
    {
        if (farmsBuildPanel != null)   farmsBuildPanel.SetActive(false);
        if (unitTrainingBuildPanel != null)    unitTrainingBuildPanel.SetActive(true);
        if (miscBuildPanel != null) miscBuildPanel.SetActive(false);

        SetActiveTabVisuals(unitTrainingButton);
    }

    public void OpenMiscTab()
    {
        if (farmsBuildPanel != null)   farmsBuildPanel.SetActive(false);
        if (unitTrainingBuildPanel != null)    unitTrainingBuildPanel.SetActive(false);
        if (miscBuildPanel != null) miscBuildPanel.SetActive(true);

        SetActiveTabVisuals(miscButton);
    }

    private void CacheOriginalColors()
    {
        if (colorsCached) return;

        if (farmsButton != null)   originalFarmsColor = farmsButton.image.color;
        if (unitTrainingButton != null)    originalUnitTrainingColor = unitTrainingButton.image.color;
        if (miscButton != null) originalMiscColor = miscButton.image.color;

        colorsCached = true;
    }

    private void SetActiveTabVisuals(Button activeBtn)
    {
        CacheOriginalColors();

        // Multiplies the original colors by a dim fraction for background tabs
        if (farmsButton != null)   farmsButton.image.color = originalFarmsColor * new Color(inactiveAlphaOrDim, inactiveAlphaOrDim, inactiveAlphaOrDim, 1f);
        if (unitTrainingButton != null)    unitTrainingButton.image.color = originalUnitTrainingColor * new Color(inactiveAlphaOrDim, inactiveAlphaOrDim, inactiveAlphaOrDim, 1f);
        if (miscButton != null) miscButton.image.color = originalMiscColor * new Color(inactiveAlphaOrDim, inactiveAlphaOrDim, inactiveAlphaOrDim, 1f);

        // Restores full original beauty to only the active slot
        if (activeBtn == farmsButton && farmsButton != null)     farmsButton.image.color = originalFarmsColor;
        if (activeBtn == unitTrainingButton && unitTrainingButton != null)       unitTrainingButton.image.color = originalUnitTrainingColor;
        if (activeBtn == miscButton && miscButton != null) miscButton.image.color = originalMiscColor;
    }

    // --- EXISTING HOOKS AND WORKFLOWS ---

    public void ShowResourceValues(PlayerSO playerSO)
    {
        if (playerSO == null) return;
        if (woodText != null) woodText.text = playerSO.woodResources.ToString();
        if (stoneText != null) stoneText.text = playerSO.stoneResources.ToString();
        if (farmText != null) farmText.text = playerSO.farmResources.ToString();
        if (energyText != null) energyText.text = playerSO.energyPoints.ToString();
        if (researchText != null) researchText.text = playerSO.researchPoints.ToString();
        if (gemsText != null) gemsText.text = playerSO.gems.ToString();
        if (coinsText != null) coinsText.text = playerSO.coins.ToString();
    }

    public void ShowSelectedBuilding(BuildingData data)
    {
        if (data == null) return;
        var panel = EnsureObjectInfoPanel();
        if (panel != null)
        {
            currentObjectInfoPanel.SetActive(true);
            panel.Setup(data);
        }
    }

    public void ShowSelectedTroop(TroopData troop)
    {
        if (troop == null) return;
        var panel = EnsureObjectInfoPanel();
        if (panel != null)
        {
            currentObjectInfoPanel.SetActive(true);
            panel.Setup(troop);
        }
    }

    public void ShowObjectInfo(Building building)
    {
        if (building == null) return;

        var panel = EnsureObjectInfoPanel();

        if (panel != null)
        {
            currentObjectInfoPanel.SetActive(true);
            panel.Setup(building);
        }
    }

    public void CloseObjectInfo()
    {
        if (currentObjectInfoPanel == null)
            return;

        if (currentObjectInfoPanelIsInstantiated)
        {
            Destroy(currentObjectInfoPanel);
            currentObjectInfoPanel = null;
            currentObjectInfoPanelIsInstantiated = false;
        }
        else
        {
            currentObjectInfoPanel.SetActive(false);
        }
    }

    public bool IsObjectInfoOpen => currentObjectInfoPanel != null;

    private BuildingInfoPanel EnsureObjectInfoPanel()
    {
        if (currentObjectInfoPanel != null)
        {
            var existingPanel = currentObjectInfoPanel.GetComponent<BuildingInfoPanel>();
            if (existingPanel != null)
            {
                return existingPanel;
            }
        }

        if (objectInfoParent == null)
        {
            Debug.LogWarning("KingdomUIManager: missing objectInfoParent.");
            return null;
        }

        if (buildingInfoPrefab == null)
        {
            Debug.LogWarning("KingdomUIManager: missing buildingInfoPrefab.");
            return null;
        }

        currentObjectInfoPanel = Instantiate(buildingInfoPrefab, objectInfoParent, false);
        currentObjectInfoPanelIsInstantiated = true;

        var panel = currentObjectInfoPanel.GetComponent<BuildingInfoPanel>();
        if (panel == null)
        {
            Debug.LogWarning("KingdomUIManager: buildingInfoPrefab does not have BuildingInfoPanel on the root.");
        }

        return panel;
    }

    public void OpenTroopSelectionPanel()
    {
        Debug.Log("Opening troop selection panel");
        if (troopSelectionPanel != null)
            troopSelectionPanel.SetActive(true);
    }

    public void CloseTroopSelectionPanel()
    {
        if (troopSelectionPanel != null)
            troopSelectionPanel.SetActive(false);
    }

    public void ToggleTroopSelectionPanel()
    {
        Debug.Log("Toggling troop selection panel");
        if (troopSelectionPanel != null)
            troopSelectionPanel.SetActive(!troopSelectionPanel.activeSelf);
    }

    public void OpenBagPanel()
    {
        Debug.Log("Opening bag panel");
        if (bagPanel != null && !bagPanel.activeSelf)
            bagPanel.SetActive(true);
    }

    public void CloseBagPanel()
    {
        if (bagPanel != null && bagPanel.activeSelf)
            bagPanel.SetActive(false);
    }

    public void ToggleBagPanel()
    {
        Debug.Log("Toggling bag panel");
        if (bagPanel != null)
            bagPanel.SetActive(!bagPanel.activeSelf);
    }

    public void LoadTargetScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}