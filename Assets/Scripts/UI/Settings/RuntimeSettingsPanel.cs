using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RuntimeSettingsPanel : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Individual Mute Toggles")]
    [SerializeField] private Toggle masterMuteToggle;
    [SerializeField] private Toggle musicMuteToggle;
    [SerializeField] private Toggle sfxMuteToggle;

    [Header("Navigation Buttons")]
    [Tooltip("Assign your Back or Close button here to disable this settings panel.")]
    [SerializeField] private Button backButton;

    [Header("Save Button Setup")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Image saveButtonImage; 
    [SerializeField] private Color saveSuccessColor = Color.green;
    [SerializeField] private float colorFlashDuration = 0.8f;

    private Color originalButtonColor;
    private Coroutine flashCoroutine;
    private bool hasSavedInThisSession = false;

    private void Awake()
    {
        if (saveButtonImage != null)
        {
            originalButtonColor = saveButtonImage.color;
        }

        if (saveButton != null)
        {
            saveButton.onClick.AddListener(OnSaveButtonClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
    }

    private void OnEnable()
    {
        hasSavedInThisSession = false;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.BindRuntimeSliders(
                masterSlider, musicSlider, sfxSlider, 
                masterMuteToggle, musicMuteToggle, sfxMuteToggle
            );
        }
    }

    private void OnDisable()
    {
        if (!hasSavedInThisSession && SoundManager.Instance != null)
        {
            SoundManager.Instance.Invoke("Start", 0f); 
        }
    }

    private void OnSaveButtonClicked()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SaveVolumeParams();
            hasSavedInThisSession = true;
        }

        if (saveButtonImage != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashSaveButtonGreen());
        }
    }

    private void OnBackButtonClicked()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator FlashSaveButtonGreen()
    {
        saveButtonImage.color = saveSuccessColor;

        float elapsed = 0f;
        while (elapsed < colorFlashDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            saveButtonImage.color = Color.Lerp(saveSuccessColor, originalButtonColor, elapsed / colorFlashDuration);
            yield return null;
        }

        saveButtonImage.color = originalButtonColor;
    }
}