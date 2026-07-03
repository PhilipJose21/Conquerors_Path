using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Configurations")]
    [SerializeField] private string mainKingdomSceneName = "MainKingdom";

    [Header("Sub Panels")]
    [SerializeField] private GameObject settingsPanel;

    public void PlayGame()
    {
        Debug.Log("Loading Main Kingdom Scene...");
        // Make sure your MainKingdomScene is added to Build Settings!
        SceneManager.LoadScene(mainKingdomSceneName);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            Debug.Log("Settings Panel Opened.");
        }
    }
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            Debug.Log("Settings Panel Closed.");
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game Application...");
        Application.Quit();

        #if UNITY_EDITOR
        // This stops play mode inside the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    } 
}    