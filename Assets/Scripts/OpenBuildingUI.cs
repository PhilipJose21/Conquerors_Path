using UnityEngine;

public class OpenBuildingUI : MonoBehaviour
{
    public GameObject buildingUIPrefab;
    public Transform transformUI;
    bool isUIOpen = false;

    public void Awake()
    {
        transformUI = GameObject.FindWithTag("ObjectInformationParent").transform;
    }

    public void OpenUI()
    {
        if (buildingUIPrefab != null && !isUIOpen)
        {
            // Instantiate the UI as a child and keep the prefab's local RectTransform values
            GameObject go = Instantiate(buildingUIPrefab);
            go.transform.SetParent(transformUI, false); // false = keep local transform (anchoredPosition, scale)
            isUIOpen = true;
        }
        else
        {
            Debug.LogWarning("Building UI Prefab is not assigned.");
        }
    }
}
