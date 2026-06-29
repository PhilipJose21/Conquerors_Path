using UnityEngine;

public class SystemMenuController : MonoBehaviour
{
    [Header("Menu Buttons Container")]
    [SerializeField] private GameObject menuButtonsParent; 

    private bool isMenuOpen = true;
    private bool isMuted = false;

    // Element 3: Main toggle button logic
    public void ToggleMenuVisibility()
    {
        isMenuOpen = !isMenuOpen;
        if (menuButtonsParent != null)
        {
            menuButtonsParent.SetActive(isMenuOpen);
        }
        Debug.Log("Menu toggled. Visible: " + isMenuOpen);
    }

    // Element 8: Advanced Settings
    public void OpenAdvancedSettings()
    {
        Debug.Log("Opening Advanced Settings Panel...");
        // TODO: Instantiate or SetActive(true) your settings overlay canvas here
    }

    // Element 9: Mute Audio
    public void ToggleMute()
    {
        isMuted = !isMuted;
        
        // Mute master volume depends on your audio engine setup (AudioListener or custom SoundManager)
        AudioListener.pause = isMuted; 
        
        Debug.Log("Audio Mute State Toggled! Is Muted: " + isMuted);
    }

    // Element 10: Surrender Behavior
    public void ExecuteSurrender()
    {
        Debug.Log("Player chooses to Surrender! Ending match...");

        // Connect straight to your project's TurnManager framework to trigger a loss state
        TurnManager turnManager = Object.FindAnyObjectByType<TurnManager>();
        if (turnManager != null)
        {
            // If your TurnManager has a defeat screen or game over trigger, fire it right here!
            // e.g., turnManager.TriggerGameOver(false);
            
            // For now, let's look at your screen references:
            Debug.Log("Loading Defeat/Game Over Screen UI...");
        }
    }
}