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
        // Route all building selections through the shared UI manager.
        var building = GetComponent<Building>() ?? GetComponentInParent<Building>();
        if (building != null)
        {
            GameObject uiInstance = Instantiate(buildingUIPrefab, transformUI);

            uiInstance.GetComponent<BuildingInformationPanel>().buildingData = this.GetComponent<BuildingStatContainer>().buildingStatsSO;
            uiInstance.GetComponent<BuildingInformationPanel>().gameObjectParent = building.gameObject;
            return;
        }

        Debug.LogWarning("OpenBuildingUI: no Building found or KingdomUIManager is missing.");
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
