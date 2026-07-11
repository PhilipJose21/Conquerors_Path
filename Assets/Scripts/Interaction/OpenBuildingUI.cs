using UnityEngine;

public class OpenBuildingUI : MonoBehaviour
{
    public GameObject buildingUIPrefab;
    public Transform transformUI;
    bool isUIOpen = false;

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

        BuildingStatContainer statContainer = GetComponent<BuildingStatContainer>();

        // InfoPanel is a singleton (see InfoPanel.Awake) - there is exactly one persistent
        // instance already placed in the scene (KingdomUIManager.sceneInfoPanel). If this
        // building's UI prefab is that same InfoPanel prefab, DO NOT instantiate a second
        // copy - any duplicate InfoPanel destroys itself in Awake() the instant it's created,
        // since Instance is already claimed by the scene copy. Instead, just reuse the
        // existing singleton and feed it this building's data.
        if (buildingUIPrefab != null && buildingUIPrefab.GetComponent<InfoPanel>() != null)
        {
            // Go through KingdomUIManager's own reference rather than InfoPanel.Instance.
            // Instance is only populated once InfoPanel.Awake() has actually run, which
            // never happens if the panel GameObject starts inactive in the scene.
            // KingdomUIManager finds it via FindObjectsInactive.Include, so it works
            // regardless of the panel's active state.
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

        // Non-singleton UI (e.g. TrainUpgradeTroopUI) - safe to clear old instances and
        // instantiate a fresh copy each time.
        if (transformUI != null)
        {
            for (int i = transformUI.childCount - 1; i >= 0; i--)
            {
                Destroy(transformUI.GetChild(i).gameObject);
            }
        }

        GameObject uiInstance = Instantiate(buildingUIPrefab, transformUI);

        TrainUpgradeTroopUI trainUpgradeTroopUI = uiInstance.GetComponent<TrainUpgradeTroopUI>() ?? uiInstance.GetComponentInChildren<TrainUpgradeTroopUI>(true);
        if (trainUpgradeTroopUI == null)
        {
            trainUpgradeTroopUI = Object.FindFirstObjectByType<TrainUpgradeTroopUI>();
        }

        if (trainUpgradeTroopUI != null)
        {
            trainUpgradeTroopUI.gameObjectParent = building.gameObject;
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