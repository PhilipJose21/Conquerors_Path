using UnityEngine;

public class OpenBuildingUI : MonoBehaviour
{
    public GameObject buildingUIPrefab;
    public Transform transformUI;
    private bool isUIOpen = false;

    public void Awake()
    {
        if (transformUI == null)
        {
            var go = GameObject.FindWithTag("ObjectInformationParent");
            if (go != null) transformUI = go.transform;
        }
    }

    public void OpenUI()
    {
        var building = GetComponent<Building>() ?? GetComponentInParent<Building>();
        if (building == null)
        {
            Debug.LogWarning("OpenBuildingUI: no Building found.");
            return;
        }

        // Search current GameObject, children, or parents for BuildingStatContainer
        BuildingStatContainer statContainer = GetComponent<BuildingStatContainer>() 
            ?? GetComponentInChildren<BuildingStatContainer>() 
            ?? GetComponentInParent<BuildingStatContainer>();

        if (statContainer == null)
        {
            Debug.LogWarning($"OpenBuildingUI on {gameObject.name}: No BuildingStatContainer found!");
        }

        // InfoPanel handling
        if (buildingUIPrefab != null && buildingUIPrefab.GetComponent<InfoPanel>() != null)
        {
            InfoPanel infoPanel = KingdomUIManager.Instance != null ? KingdomUIManager.Instance.sceneInfoPanel : null;
            if (infoPanel == null)
            {
                infoPanel = Object.FindFirstObjectByType<InfoPanel>(FindObjectsInactive.Include);
            }
            if (infoPanel == null)
            {
                Debug.LogWarning("OpenBuildingUI: no InfoPanel found in the scene.");
                return;
            }

            infoPanel.gameObject.SetActive(true);
            infoPanel.buildingData = statContainer?.buildingData;
            infoPanel.gameObjectParent = building.gameObject;
            infoPanel.SetUp(statContainer?.buildingStatsSO, null);

            isUIOpen = true;
            return;
        }

        // Clear existing UI
        if (transformUI != null)
        {
            for (int i = transformUI.childCount - 1; i >= 0; i--)
            {
                Destroy(transformUI.GetChild(i).gameObject);
            }
        }

        GameObject uiInstance = Instantiate(buildingUIPrefab, transformUI);

        // --- Pass Building Data to TrainUpgradeTroopUI ---
        TrainUpgradeTroopUI trainUpgradeTroopUI = uiInstance.GetComponentInChildren<TrainUpgradeTroopUI>(true);
        if (trainUpgradeTroopUI != null)
        {
            trainUpgradeTroopUI.gameObjectParent = building.gameObject;
            trainUpgradeTroopUI.SetBuildingData(statContainer?.buildingData);
        }

        // --- Pass Building Data to UnlockUnits ---
        UnlockUnits unlockUnits = uiInstance.GetComponentInChildren<UnlockUnits>(true);
        if (unlockUnits != null)
        {
            unlockUnits.SetBuildingData(statContainer?.buildingData);
        }

        isUIOpen = true;
    }

    public void CloseUI()
    {
        if (KingdomUIManager.Instance != null)
        {
            KingdomUIManager.Instance.CloseObjectInfo();
        }
        isUIOpen = false;
    }

    public bool IsUIOpen => isUIOpen;
}