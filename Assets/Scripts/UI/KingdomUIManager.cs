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

    [Header("Object Info / Building Panel (In-Scene Reference)")]
    [Tooltip("Drag the existing InfoPanel from your Hierarchy straight into this slot!")]
    public InfoPanel sceneInfoPanel;
    
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

    [Header("Build Mode Smooth Animation Settings")]
    [SerializeField] private float slideDuration = 0.25f; 
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Settings")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button settingsButton;

    private Color originalFarmsColor;
    private Color originalUnitTrainingColor;
    private Color originalMiscColor;
    private bool colorsCached = false;

    // Animation tracking variables
    private Coroutine slideCoroutine;
    private Vector2 panelHiddenPosition;
    private Vector2 panelShownPosition;
    private RectTransform buildModeRectTransform;

    [Header("Scene Management")]
    public string worldSelectScenename = "Level Select";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Fallback lookup if you forget to drag it into the inspector slot
        if (sceneInfoPanel == null)
        {
            sceneInfoPanel = Object.FindFirstObjectByType<InfoPanel>(FindObjectsInactive.Include);
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
        if (buildModeToggleButton != null) buildModeToggleButton.onClick.AddListener(ToggleBuildMode);
        
        if (farmsButton != null)   farmsButton.onClick.AddListener(OpenFarmsTab);
        if (unitTrainingButton != null)    unitTrainingButton.onClick.AddListener(OpenUnitTrainingTab);
        if (miscButton != null) miscButton.onClick.AddListener(OpenMiscTab);

        if (playerSO != null) ShowResourceValues(playerSO);
        
        InitializeAnimationCoordinates();
        OpenFarmsTab();

        // Ensure the info panel starts closed when the game boots
        if (sceneInfoPanel != null) sceneInfoPanel.gameObject.SetActive(false);
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

    // --- ANIMATED SLIDE UP & DOWN SYSTEM ---

    private void InitializeAnimationCoordinates()
    {
        if (buildModePanel == null) return;

        buildModeRectTransform = buildModePanel.GetComponent<RectTransform>();
        if (buildModeRectTransform != null)
        {
            panelShownPosition = buildModeRectTransform.anchoredPosition;
            panelHiddenPosition = new Vector2(panelShownPosition.x, panelShownPosition.y - buildModeRectTransform.rect.height - 150f);

            if (!buildModePanel.activeSelf)
            {
                buildModeRectTransform.anchoredPosition = panelHiddenPosition;
            }
        }
    }

    public void ToggleBuildMode()
    {
        if (buildModePanel == null || buildModeRectTransform == null) return;

        if (slideCoroutine != null) StopCoroutine(slideCoroutine);

        if (!buildModePanel.activeSelf || buildModeRectTransform.anchoredPosition == panelHiddenPosition)
        {
            buildModePanel.SetActive(true);
            slideCoroutine = StartCoroutine(SlidePanel(buildModeRectTransform.anchoredPosition, panelShownPosition, true));
        }
        else
        {
            slideCoroutine = StartCoroutine(SlidePanel(buildModeRectTransform.anchoredPosition, panelHiddenPosition, false));
        }
    }

    private System.Collections.IEnumerator SlidePanel(Vector2 startPos, Vector2 endPos, bool keepActiveAtEnd)
    {
        float elapsedTime = 0f;

        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; 
            float t = elapsedTime / slideDuration;
            float curvedT = slideCurve.Evaluate(t);

            buildModeRectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, curvedT);
            yield return null;
        }

        buildModeRectTransform.anchoredPosition = endPos;

        if (!keepActiveAtEnd)
        {
            buildModePanel.SetActive(false);
        }
    }

    // --- BUILD MODE CATEGORY SELECTION ---

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

        if (farmsButton != null)   farmsButton.image.color = originalFarmsColor * new Color(inactiveAlphaOrDim, inactiveAlphaOrDim, inactiveAlphaOrDim, 1f);
        if (unitTrainingButton != null)    unitTrainingButton.image.color = originalUnitTrainingColor * new Color(inactiveAlphaOrDim, inactiveAlphaOrDim, inactiveAlphaOrDim, 1f);
        if (miscButton != null) miscButton.image.color = originalMiscColor * new Color(inactiveAlphaOrDim, inactiveAlphaOrDim, inactiveAlphaOrDim, 1f);

        if (activeBtn == farmsButton && farmsButton != null)     farmsButton.image.color = originalFarmsColor;
        if (activeBtn == unitTrainingButton && unitTrainingButton != null)       unitTrainingButton.image.color = originalUnitTrainingColor;
        if (activeBtn == miscButton && miscButton != null) miscButton.image.color = originalMiscColor;
    }

    // --- CLEANED SHOW/HIDE FLOWS ---

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
        if (data == null || sceneInfoPanel == null) return;
        
        sceneInfoPanel.gameObject.SetActive(true);
        sceneInfoPanel.buildingData = data; 
        sceneInfoPanel.SetUp(null, null);
    }

    public void ShowSelectedTroop(TroopData troop)
    {
        if (troop == null || sceneInfoPanel == null) return;
        
        sceneInfoPanel.gameObject.SetActive(true);
        sceneInfoPanel.SetUp(null, troop);
    }

    public void ShowObjectInfo(Building building)
    {
        if (building == null || sceneInfoPanel == null) return;

        sceneInfoPanel.gameObject.SetActive(true);
        sceneInfoPanel.SetUp(null, null); 
    }

    public void CloseObjectInfo()
    {
        if (sceneInfoPanel != null)
        {
            sceneInfoPanel.gameObject.SetActive(false);
        }
    }

    public bool IsObjectInfoOpen => sceneInfoPanel != null && sceneInfoPanel.gameObject.activeSelf;

    // --- OTHER PANELS SYSTEM ---

    public void OpenTroopSelectionPanel()
    {
        if (troopSelectionPanel != null) troopSelectionPanel.SetActive(true);
    }

    public void CloseTroopSelectionPanel()
    {
        if (troopSelectionPanel != null) troopSelectionPanel.SetActive(false);
    }

    public void ToggleTroopSelectionPanel()
    {
        if (troopSelectionPanel != null) boxPanelState(!troopSelectionPanel.activeSelf);
    }

    private void boxPanelState(bool state)
    {
        if (troopSelectionPanel != null) troopSelectionPanel.SetActive(state);
    }

    public void OpenBagPanel()
    {
        if (bagPanel != null && !bagPanel.activeSelf) bagPanel.SetActive(true);
    }

    public void CloseBagPanel()
    {
        if (bagPanel != null && bagPanel.activeSelf) bagPanel.SetActive(false);
    }

    public void ToggleBagPanel()
    {
        if (bagPanel != null) bagPanel.SetActive(!bagPanel.activeSelf);
    }

    public void OpenSettingsPanel()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettingsPanel()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }
    
    public void ToggleSettingsPanel()
    {
        if (settingsPanel != null) settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void LoadTargetScene(string sceneName)
    {
        KingdomSaveManager.Instance?.SaveCurrentKingdom();
        SceneManager.LoadScene(sceneName);
    }

    public void loadWorldSelectScene()
    {
        KingdomSaveManager.Instance?.SaveCurrentKingdom();
        SceneManager.LoadScene(worldSelectScenename);
    }
}