using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct TutorialPage
{
    public string pageTitle;

    [TextArea(3, 5)]
    public string pageDescription;

    public Sprite illustrationImage;
}

public class TutorialManager : MonoBehaviour
{
    private const string TutorialStepKey = "TutorialStep";
    private const string TutorialCompletedKey = "TutorialCompleted";
    private const string NextLabel = "Next";
    private const string CloseLabel = "Close";

    [Header("Scene Step Config")]
    [SerializeField] private int requiredStepIndex;
    [SerializeField] private int stepToSetOnCompletion = 1;
    [SerializeField] private bool markTutorialAsFullyCompleted = false;

    [Header("Contextual Trigger Settings")]
    [Tooltip("If true, automatically opens the tutorial after triggerDelay seconds upon scene start or panel enable.")]
    [SerializeField] private bool autoTriggerOnStart = true;
    [Tooltip("Delay in seconds before showing the tutorial when auto-triggered.")]
    [SerializeField] private float triggerDelay = 0.5f;

    [Header("Tutorial UI")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image illustrationImage;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;

    [Header("Tutorial Pages")]
    [SerializeField] private List<TutorialPage> pages = new List<TutorialPage>();

    private int currentPageIndex;
    private TMP_Text nextButtonLabel;
    private bool hasValidated;
    private bool isInitialized;
    private Coroutine delayCoroutine;

    private void Awake()
    {
        CacheButtonLabel();
        ValidateReferences();
    }

    private void OnEnable()
    {
        EvaluateTutorialState();
    }

    private void OnDisable()
    {
        UnbindButtonEvents();
        if (delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
            delayCoroutine = null;
        }
    }

    public void EvaluateTutorialState()
    {
        int currentSavedStep = PlayerPrefs.GetInt(TutorialStepKey, 0);
        int isCompleted = PlayerPrefs.GetInt(TutorialCompletedKey, 0);

        // Don't trigger if tutorial is already finished globally or required step doesn't match
        if (isCompleted == 1 || currentSavedStep != requiredStepIndex)
        {
            SetPanelVisible(false);
            return;
        }

        currentPageIndex = 0;
        isInitialized = true;

        if (autoTriggerOnStart)
        {
            TriggerTutorialWithDelay(triggerDelay);
        }
    }

    private void ValidateReferences()
    {
        if (hasValidated) return;
        hasValidated = true;

        if (tutorialPanel == null) Debug.LogError($"{nameof(TutorialManager)} on {name}: Missing tutorialPanel reference.", this);
        if (titleText == null) Debug.LogError($"{nameof(TutorialManager)} on {name}: Missing titleText reference.", this);
        if (descriptionText == null) Debug.LogError($"{nameof(TutorialManager)} on {name}: Missing descriptionText reference.", this);
        if (illustrationImage == null) Debug.LogError($"{nameof(TutorialManager)} on {name}: Missing illustrationImage reference.", this);
        if (nextButton == null) Debug.LogError($"{nameof(TutorialManager)} on {name}: Missing nextButton reference.", this);
        if (prevButton == null) Debug.LogError($"{nameof(TutorialManager)} on {name}: Missing prevButton reference.", this);
    }

    private void CacheButtonLabel()
    {
        if (nextButton != null)
        {
            nextButtonLabel = nextButton.GetComponentInChildren<TMP_Text>(true);
        }
    }

    private void BindButtonEvents()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(HandleNextClicked);
            nextButton.onClick.AddListener(HandleNextClicked);
        }

        if (prevButton != null)
        {
            prevButton.onClick.RemoveListener(HandlePreviousClicked);
            prevButton.onClick.AddListener(HandlePreviousClicked);
        }
    }

    private void UnbindButtonEvents()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(HandleNextClicked);
        }

        if (prevButton != null)
        {
            prevButton.onClick.RemoveListener(HandlePreviousClicked);
        }
    }

    public void TriggerTutorial()
    {
        currentPageIndex = 0;
        SetPanelVisible(true);
        UpdatePageUI();
    }

    public void TriggerTutorialWithDelay(float delay)
    {
        if (delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
        }

        delayCoroutine = StartCoroutine(ShowTutorialAfterDelay(delay));
    }

    private IEnumerator ShowTutorialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        TriggerTutorial();
        delayCoroutine = null;
    }

    public void HandleNextClicked()
    {
        if (pages == null || pages.Count == 0)
        {
            CompleteCurrentStep();
            return;
        }

        if (currentPageIndex < pages.Count - 1)
        {
            currentPageIndex++;
            UpdatePageUI();
            return;
        }

        CompleteCurrentStep();
    }

    public void HandlePreviousClicked()
    {
        if (currentPageIndex <= 0) return;

        currentPageIndex--;
        UpdatePageUI();
    }

    private void UpdatePageUI()
    {
        if (pages == null || pages.Count == 0)
        {
            SetPageText(string.Empty, string.Empty);
            SetIllustration(null);
            SetPrevButtonVisible(false);
            SetNextButtonLabel(NextLabel);
            return;
        }

        currentPageIndex = Mathf.Clamp(currentPageIndex, 0, pages.Count - 1);
        TutorialPage page = pages[currentPageIndex];

        SetPageText(page.pageTitle, page.pageDescription);
        SetIllustration(page.illustrationImage);
        SetPrevButtonVisible(currentPageIndex > 0);
        SetNextButtonLabel(currentPageIndex == pages.Count - 1 ? CloseLabel : NextLabel);
    }

    private void SetPageText(string pageTitle, string pageDescription)
    {
        if (titleText != null) titleText.text = pageTitle;
        if (descriptionText != null) descriptionText.text = pageDescription;
    }

    private void SetIllustration(Sprite illustrationSprite)
    {
        if (illustrationImage == null) return;

        illustrationImage.sprite = illustrationSprite;
        bool hasImage = illustrationSprite != null;
        illustrationImage.enabled = hasImage;
    }

    private void SetPrevButtonVisible(bool isVisible)
    {
        if (prevButton != null)
        {
            prevButton.gameObject.SetActive(isVisible);
        }
    }

    private void SetNextButtonLabel(string label)
    {
        if (nextButtonLabel != null)
        {
            nextButtonLabel.text = label;
        }
    }

    private void SetPanelVisible(bool isVisible)
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(isVisible);
        }

        // Dynamically manage button listeners so inactive scripts never capture events
        if (isVisible)
        {
            BindButtonEvents();
        }
        else
        {
            UnbindButtonEvents();
        }
    }

    private void CompleteCurrentStep()
    {
        PlayerPrefs.SetInt(TutorialStepKey, stepToSetOnCompletion);

        if (markTutorialAsFullyCompleted)
        {
            PlayerPrefs.SetInt(TutorialCompletedKey, 1);
        }

        PlayerPrefs.Save();
        SetPanelVisible(false);
    }

    /// <summary>
    /// Call this via Context Menu (3 dots on component) or Editor Script to clear state during testing.
    /// </summary>
    [ContextMenu("Reset Tutorial Progress")]
    public void ResetTutorialProgress()
    {
        PlayerPrefs.SetInt(TutorialStepKey, 0);
        PlayerPrefs.SetInt(TutorialCompletedKey, 0);
        PlayerPrefs.Save();
        Debug.Log("[TutorialManager] Tutorial PlayerPrefs progress reset to 0.");
    }

    public void ResetTutorialProgressAndShow()
    {
        ResetTutorialProgress();
        TriggerTutorial();
    }
}