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

    private void OnEnable()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.BindRuntimeSliders(
                masterSlider, musicSlider, sfxSlider, 
                masterMuteToggle, musicMuteToggle, sfxMuteToggle
            );
        }
    }
}