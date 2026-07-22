using UnityEngine;

public class OpenUI : MonoBehaviour
{
    public GameObject uiToOpen;

    public void toggleUI()
    {
        if (uiToOpen != null)
        {
            uiToOpen.SetActive(!uiToOpen.activeSelf);
        }
    }
}
