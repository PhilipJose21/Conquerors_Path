using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Buttons")]
    [SerializeField] private Button saveVolumes;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Volume Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Persistent Music Source Hooks")]
    [SerializeField] private AudioSource musicAudioSource; 
    public AudioClip mainKingdomMusic;                     
    public AudioClip levelSelectMusic;                    

    private AudioSource sfxAudioSource;

    private float masterVol;
    private float musicVol;
    private float sfxVol;

    // UPDATED: Individual channel mute variables
    private bool isMasterMuted = false;
    private bool isMusicMuted = false;
    private bool isSfxMuted = false;

    private const string MASTERVOLUME = "masterVolume";
    private const string MUSICVOLUME  = "musicVolume";
    private const string SFXVOLUME    = "sfxVolume";
    
    // UPDATED: Keys for tracking individual channel mutes
    private const string MASTERMUTEP_PREF = "masterMuted";
    private const string MUSICMUTEP_PREF  = "musicMuted";
    private const string SFXMUTEP_PREF    = "sfxMuted";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); 

        if (musicAudioSource == null)
        {
            musicAudioSource = GetComponent<AudioSource>();
            if (musicAudioSource == null) musicAudioSource = gameObject.AddComponent<AudioSource>();
        }
        musicAudioSource.loop = true;
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void Start()
    {
        LoadVolumeParams();
        
        if (transform.childCount > 1)
        {
            sfxAudioSource = transform.GetChild(1).GetComponent<AudioSource>();
        }

        // Apply loaded parameters to Mixer groups
        MasterVolume(masterVol);
        MusicVolume(musicVol);
        SFXVolume(sfxVol);

        // Force explicit updates to handle loaded mute states on boot
        ToggleMasterMute(isMasterMuted);
        ToggleMusicMute(isMusicMuted);
        ToggleSFXMute(isSfxMuted);

        if (saveVolumes != null) saveVolumes.onClick.AddListener(SaveVolumeParams);

        EvaluateSceneBGM(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EvaluateSceneBGM(scene.name);
    }

    private void EvaluateSceneBGM(string sceneName)
    {
        switch (sceneName)
        {
            case "MainMenu":
            case "MainKingdom":
                PlayBGM(mainKingdomMusic);
                break;
            case "Level Select":
                PlayBGM(levelSelectMusic);
                break;
        }
    }

    private void PlayBGM(AudioClip trackClip)
    {
        if (trackClip == null || musicAudioSource == null) return;
        if (musicAudioSource.clip == trackClip && musicAudioSource.isPlaying) return;
        musicAudioSource.clip = trackClip;
        musicAudioSource.Play();
    }

    // UPDATED: Central registration system handles all three individual toggles
    public void BindRuntimeSliders(Slider master, Slider music, Slider sfx, Toggle masterMute, Toggle musicMute, Toggle sfxMute)
    {
        masterSlider = master;
        musicSlider = music;
        sfxSlider = sfx;

        // Sliders binding loop
        if (masterSlider != null) { masterSlider.value = masterVol; masterSlider.onValueChanged.RemoveAllListeners(); masterSlider.onValueChanged.AddListener(MasterVolume); }
        if (musicSlider != null) { musicSlider.value = musicVol; musicSlider.onValueChanged.RemoveAllListeners(); musicSlider.onValueChanged.AddListener(MusicVolume); }
        if (sfxSlider != null) { sfxSlider.value = sfxVol; sfxSlider.onValueChanged.RemoveAllListeners(); sfxSlider.onValueChanged.AddListener(SFXVolume); }

        // Toggles binding loop
        if (masterMute != null) { masterMute.isOn = isMasterMuted; masterMute.onValueChanged.RemoveAllListeners(); masterMute.onValueChanged.AddListener(ToggleMasterMute); }
        if (musicMute != null) { musicMute.isOn = isMusicMuted; musicMute.onValueChanged.RemoveAllListeners(); musicMute.onValueChanged.AddListener(ToggleMusicMute); }
        if (sfxMute != null) { sfxMute.isOn = isSfxMuted; sfxMute.onValueChanged.RemoveAllListeners(); sfxMute.onValueChanged.AddListener(ToggleSFXMute); }
    }

    private void SaveVolumeParams()
    {
        PlayerPrefs.SetFloat(MASTERVOLUME, masterVol);
        PlayerPrefs.SetFloat(MUSICVOLUME, musicVol);
        PlayerPrefs.SetFloat(SFXVOLUME, sfxVol);
        PlayerPrefs.SetInt(MASTERMUTEP_PREF, isMasterMuted ? 1 : 0);
        PlayerPrefs.SetInt(MUSICMUTEP_PREF, isMusicMuted ? 1 : 0);
        PlayerPrefs.SetInt(SFXMUTEP_PREF, isSfxMuted ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log("Audio configuration saved successfully!");
    }

    private void LoadVolumeParams()
    {
        masterVol = PlayerPrefs.GetFloat(MASTERVOLUME, 1f);
        musicVol  = PlayerPrefs.GetFloat(MUSICVOLUME, 1f);
        sfxVol    = PlayerPrefs.GetFloat(SFXVOLUME, 1f);
        isMasterMuted = PlayerPrefs.GetInt(MASTERMUTEP_PREF, 0) == 1;
        isMusicMuted  = PlayerPrefs.GetInt(MUSICMUTEP_PREF, 0) == 1;
        isSfxMuted    = PlayerPrefs.GetInt(SFXMUTEP_PREF, 0) == 1;
    }

    public void MasterVolume(float value)
    {
        masterVol = value;
        if (!isMasterMuted && audioMixer != null)
        {
            float dB = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
            audioMixer.SetFloat(MASTERVOLUME, dB);
        }
    }

    public void MusicVolume(float value)
    {
        musicVol = value;
        if (!isMusicMuted && audioMixer != null)
        {
            float dB = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
            audioMixer.SetFloat(MUSICVOLUME, dB);
        }
    }

    public void SFXVolume(float value)
    {
        sfxVol = value;
        if (!isSfxMuted && audioMixer != null)
        {
            float dB = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
            audioMixer.SetFloat(SFXVOLUME, dB);
        }
    }

    // UPDATED: Independent mute controls per channel
    public void ToggleMasterMute(bool muteState)
    {
        isMasterMuted = muteState;
        if (audioMixer != null)
        {
            if (isMasterMuted) audioMixer.SetFloat(MASTERVOLUME, -80f);
            else MasterVolume(masterVol);
        }
    }

    public void ToggleMusicMute(bool muteState)
    {
        isMusicMuted = muteState;
        if (audioMixer != null)
        {
            if (isMusicMuted) audioMixer.SetFloat(MUSICVOLUME, -80f);
            else MusicVolume(musicVol);
        }
    }

    public void ToggleSFXMute(bool muteState)
    {
        isSfxMuted = muteState;
        if (audioMixer != null)
        {
            if (isSfxMuted) audioMixer.SetFloat(SFXVOLUME, -80f);
            else SFXVolume(sfxVol);
        }
    }
}