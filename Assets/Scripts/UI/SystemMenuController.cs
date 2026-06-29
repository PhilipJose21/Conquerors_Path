using UnityEngine;

public class SystemMenuController : MonoBehaviour
{
    [Header("Menu Buttons Container")]
    [SerializeField] private GameObject menuButtonsParent; 
    // Drag a GameObject here that holds buttons 8, 9, and 10 if you want to toggle their visibility!

    private bool isMenuOpen = true;

    // Element 3 Action
    public void ToggleMenuVisibility()
    {
        isMenuOpen = !isMenuOpen;
        if (menuButtonsParent != null)
        {
            menuButtonsParent.SetActive(isMenuOpen);
        }
        Debug.Log("Menu toggled. Open state: " + isMenuOpen);
    }

    // Element 8 Action
    public void OpenAdvancedSettings()
    {
        Debug.Log("Opening Advanced Settings Panel...");
    }

    // Element 9 Action
    public void ToggleMute()
    {
        Debug.Log("Toggling Audio Mute State...");
    }

    // Element 10 Action
    public void ExecuteSurrender()
    {
        Debug.Log("Player chooses to Surrender!");
    }
}