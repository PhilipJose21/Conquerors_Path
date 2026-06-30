using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button saveVolumes;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Volume Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private AudioSource sfxAudioSource;

    private float masterVol;
    private float musicVol;
    private float sfxVol;
    private bool isMuted = false;

    private const string MASTERVOLUME = "masterVolume";
    private const string MUSICVOLUME  = "musicVolume";
    private const string SFXVOLUME    = "sfxVolume";
    private const string MUTEPREF     = "isMuted";

    private void Start()
    {
        LoadVolumeParams();
        
        if (transform.childCount > 1)
        {
            sfxAudioSource = transform.GetChild(1).GetComponent<AudioSource>();
        }

        if (masterSlider != null) masterSlider.value = masterVol;
        if (musicSlider != null) musicSlider.value = musicVol;
        if (sfxSlider != null) sfxSlider.value = sfxVol;

        // Apply loaded parameters straight to the Audio Mixer upon startup
        MasterVolume(masterVol);
        MusicVolume(musicVol);
        SFXVolume(sfxVol);
        ToggleMute(isMuted);

        if (saveVolumes != null)
        {
            saveVolumes.onClick.AddListener(SaveVolumeParams);
        }
    }

    private void SaveVolumeParams()
    {
        PlayerPrefs.SetFloat(MASTERVOLUME, masterVol);
        PlayerPrefs.SetFloat(MUSICVOLUME, musicVol);
        PlayerPrefs.SetFloat(SFXVOLUME, sfxVol);
        PlayerPrefs.SetInt(MUTEPREF, isMuted ? 1 : 0); 
        PlayerPrefs.Save();
        Debug.Log("Audio configuration preferences saved successfully!");
    }

    private void LoadVolumeParams()
    {
        masterVol = PlayerPrefs.GetFloat(MASTERVOLUME, 1f);
        musicVol  = PlayerPrefs.GetFloat(MUSICVOLUME, 1f);
        sfxVol    = PlayerPrefs.GetFloat(SFXVOLUME, 1f);
        isMuted   = PlayerPrefs.GetInt(MUTEPREF, 0) == 1; 
    }

    public void MasterVolume(float value)
    {
        masterVol = value;

        if (!isMuted && audioMixer != null)
        {
            audioMixer.SetFloat(MASTERVOLUME, Mathf.Log10(value) * 20f);
        }
    }

    public void MusicVolume(float value)
    {
        musicVol = value;
        if (audioMixer != null)
        {
            audioMixer.SetFloat(MUSICVOLUME, Mathf.Log10(value) * 20f);
        }
    }

    public void SFXVolume(float value)
    {
        sfxVol = value;
        if (audioMixer != null)
        {
            audioMixer.SetFloat(SFXVOLUME, Mathf.Log10(value) * 20f);
        }
    }

    public void ToggleMute(bool muteState)
    {
        isMuted = muteState;

        if (audioMixer != null)
        {
            if (isMuted)
            {
                audioMixer.SetFloat(MASTERVOLUME, -80f);
            }
            else
            {
                MasterVolume(masterVol);
            }
        }
        Debug.Log("Master Audio Mute State updated: " + isMuted);
    }
}