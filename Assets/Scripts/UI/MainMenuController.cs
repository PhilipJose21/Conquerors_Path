using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Configurations")]
    [SerializeField] private string mainKingdomSceneName = "MainKingdom";

    [Header("Sub Panels")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Task Extensions")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;

    private void Start()
    {
        // 1. Hook up the click listeners dynamically to match the design flow
        if (continueButton != null) continueButton.onClick.AddListener(ContinueGame);
        if (newGameButton != null) newGameButton.onClick.AddListener(NewGame);

        // 2. Disable the continue button if the save file returns empty
        EvaluateSaveFileStatus();
    }

    private void EvaluateSaveFileStatus()
    {
        if (continueButton == null) return;

        bool isEmpty = KingdomSaveManager.Instance == null || KingdomSaveManager.Instance.IsSaveFileEmpty;
        
        Debug.Log($"[MainMenu] Is Save File Empty? Result: {isEmpty}");
        // Check our custom validation condition from the save manager
        if (KingdomSaveManager.Instance == null || KingdomSaveManager.Instance.IsSaveFileEmpty)
        {
            continueButton.interactable = false; // Automatically grays out and locks the UI button
        }
        else
        {
            continueButton.interactable = true;  // Unlocks it if valid resources/units exist
        }
    }

    public void ContinueGame()
    {
        Debug.Log("Continuing Game. Loading Main Kingdom Scene...");
        SceneManager.LoadScene(mainKingdomSceneName);
    }

    public void NewGame()
    {
        Debug.Log("Starting New Game. Resetting Save Data...");
        
        if (KingdomSaveManager.Instance != null)
        {
            KingdomSaveManager.Instance.ResetSaveData();

            KingdomSaveManager.Instance.SaveCurrentKingdom();

            PlayerPrefs.SetInt("IsNewGameLoading", 1);
            PlayerPrefs.Save();
        }

        SceneManager.LoadScene(mainKingdomSceneName);
    }

    public void PlayGame()
    {
        ContinueGame();
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
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    } 
}