using UnityEngine;
using UnityEngine.SceneManagement; // Required for changing scenes

public class SystemMenuController : MonoBehaviour
{
    [Header("Menu Buttons Container")]
    [SerializeField] private GameObject menuButtonsParent; 

    [Header("Surrender Confirmation Overlay")]
    [SerializeField] private GameObject surrenderConfirmationPanel; 

    private bool isMenuOpen = true;
    private bool isMuted = false;

    private void Start()
    {
        if (surrenderConfirmationPanel != null)
        {
            surrenderConfirmationPanel.SetActive(false);
        }
    }

    public void ToggleMenuVisibility()
    {
        isMenuOpen = !isMenuOpen;
        if (menuButtonsParent != null)
        {
            menuButtonsParent.SetActive(isMenuOpen);
        }
        Debug.Log("Menu toggled. Visible: " + isMenuOpen);
    }

    public void OpenAdvancedSettings()
    {
        Debug.Log("Opening Advanced Settings Panel...");
        // TODO: Instantiate or SetActive(true) your settings overlay canvas here
    }

    // Element 9: Mute Audio
    public void ToggleMute()
    {
        isMuted = !isMuted;
        
        AudioListener.pause = isMuted; 
        
        Debug.Log("Audio Mute State Toggled! Is Muted: " + isMuted);
    }

    public void ExecuteSurrender()
    {
        Debug.Log("Player clicked Surrender. Showing confirmation panel...");

        if (surrenderConfirmationPanel != null)
        {
            surrenderConfirmationPanel.SetActive(true);
        }
    }

    public void ConfirmSurrenderYes()
    {
        Debug.Log("Surrender Confirmed! Saving data and returning to Main Kingdom...");

        KingdomSaveManager.Instance?.SaveCurrentKingdom();

        SceneManager.LoadScene("MainKingdom");
    }

    // NEW: Triggered by the "NO" button on the confirmation panel
    public void ConfirmSurrenderNo()
    {
        Debug.Log("Surrender Cancelled. Returning to game menu.");

        if (surrenderConfirmationPanel != null)
        {
            surrenderConfirmationPanel.SetActive(false);
        }
    }
}