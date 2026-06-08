using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class KingdomUIManager : MonoBehaviour
{
    public static KingdomUIManager Instance { get; private set; }

    // Resource text fields (can be assigned in inspector or found at runtime)
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI stoneText;
    public TextMeshProUGUI farmText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI researchText;
    public TextMeshProUGUI gemsText;
    public TextMeshProUGUI coinsText;

    // Object info / building panel
    public Transform objectInfoParent;
    public GameObject buildingInfoPrefab;
    [SerializeField] private GameObject currentObjectInfoPanel;
    [SerializeField] private bool currentObjectInfoPanelIsInstantiated;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Find object info parent by tag if not set
        if (objectInfoParent == null)
        {
            var go = GameObject.FindWithTag("ObjectInformationParent");
            if (go != null) objectInfoParent = go.transform;
        }

        // Load building info prefab from Resources as fallback
        if (buildingInfoPrefab == null)
        {
            buildingInfoPrefab = Resources.Load<GameObject>("UI/BuildingInfoPanel");
        }

        // If the panel already exists in the scene hierarchy, reuse it instead of instantiating a second copy.
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

        // Try to find resource text fields by common paths if not assigned
        if (woodText == null) woodText = FindTMP("Canvas/ResourcePanel/WoodText");
        if (stoneText == null) stoneText = FindTMP("Canvas/ResourcePanel/StoneText");
        if (farmText == null) farmText = FindTMP("Canvas/ResourcePanel/FarmText");
        if (energyText == null) energyText = FindTMP("Canvas/ResourcePanel/EnergyText");
        if (researchText == null) researchText = FindTMP("Canvas/ResourcePanel/ResearchText");
        if (gemsText == null) gemsText = FindTMP("Canvas/ResourcePanel/GemsText");
        if (coinsText == null) coinsText = FindTMP("Canvas/ResourcePanel/CoinsText");
    }

    private TextMeshProUGUI FindTMP(string path)
    {
        var go = GameObject.Find(path);
        return go != null ? go.GetComponent<TextMeshProUGUI>() : null;
    }

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
}
