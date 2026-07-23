using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SystemMenuController : MonoBehaviour
{
    [Header("Menu Buttons Container")]
    [SerializeField] private GameObject menuButtonsParent; 

    [Header("Dropdown Animation Settings")]
    [SerializeField] private float animDuration = 0.25f;
    [SerializeField] private AnimationCurve dropdownCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Surrender Confirmation Overlay")]
    [SerializeField] private GameObject surrenderConfirmationPanel; 

    private bool isMenuOpen = false;
    private bool isMuted = false;
    private Coroutine menuAnimCoroutine;
    private RectTransform menuRectTransform;

    private void Start()
    {
        if (surrenderConfirmationPanel != null)
        {
            surrenderConfirmationPanel.SetActive(false);
        }

        if (menuButtonsParent != null)
        {
            menuRectTransform = menuButtonsParent.GetComponent<RectTransform>();

            if (menuRectTransform != null)
            {
                menuRectTransform.pivot = new Vector2(0.5f, 1f);
            }

            menuButtonsParent.transform.localScale = new Vector3(1f, 0f, 1f);
            menuButtonsParent.SetActive(false);
        }
    }

    public void ToggleMenuVisibility()
    {
        isMenuOpen = !isMenuOpen;

        if (menuButtonsParent != null)
        {
            if (menuAnimCoroutine != null)
            {
                StopCoroutine(menuAnimCoroutine);
            }

            menuAnimCoroutine = StartCoroutine(AnimateDropdown(isMenuOpen));
        }

        Debug.Log("Menu toggled. Visible: " + isMenuOpen);
    }

    private IEnumerator AnimateDropdown(bool opening)
    {
        if (opening)
        {
            menuButtonsParent.SetActive(true);
        }

        float startYScale = menuButtonsParent.transform.localScale.y;
        float targetYScale = opening ? 1f : 0f;
        float elapsed = 0f;

        while (elapsed < animDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / animDuration);
            float curveValue = dropdownCurve.Evaluate(t);

            float currentYScale = Mathf.LerpUnclamped(startYScale, targetYScale, curveValue);
            menuButtonsParent.transform.localScale = new Vector3(1f, currentYScale, 1f);

            yield return null;
        }

        menuButtonsParent.transform.localScale = new Vector3(1f, targetYScale, 1f);

        if (!opening)
        {
            menuButtonsParent.SetActive(false);
        }
    }

    public void OpenAdvancedSettings()
    {
        Debug.Log("Opening Advanced Settings Panel...");
    }

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

    public void ConfirmSurrenderNo()
    {
        Debug.Log("Surrender Cancelled. Returning to game menu.");

        if (surrenderConfirmationPanel != null)
        {
            surrenderConfirmationPanel.SetActive(false);
        }
    }
}