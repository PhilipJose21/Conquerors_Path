using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // ADDED: Necessary namespace for tracking scene shifts

public class SoundManager : MonoBehaviour
{
    // ADDED: Global Singleton Instance
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
    [SerializeField] private AudioSource musicAudioSource; // ADDED: Drag your BGM AudioSource slot here
    public AudioClip mainKingdomMusic;                     // ADDED: Kingdom BGM track
    public AudioClip levelSelectMusic;                    // ADDED: Map selection BGM track

    private AudioSource sfxAudioSource;

    private float masterVol;
    private float musicVol;
    private float sfxVol;
    private bool isMuted = false;

    private const string MASTERVOLUME = "masterVolume";
    private const string MUSICVOLUME  = "musicVolume";
    private const string SFXVOLUME    = "sfxVolume";
    private const string MUTEPREF     = "isMuted";

    private void Awake()
    {
        // ADDED: Singleton Pattern with DontDestroyOnLoad logic execution
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Ensures the script doesn't die when scenes shift!

        // Instantly verify music track routing layer properties are defined
        if (musicAudioSource == null)
        {
            musicAudioSource = GetComponent<AudioSource>();
            if (musicAudioSource == null) musicAudioSource = gameObject.AddComponent<AudioSource>();
        }
        musicAudioSource.loop = true;
    }

    private void OnEnable()
    {
        // ADDED: Subscribe to scene load handler event loops
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // ADDED: Unsubscribe when disabled to safeguard your project memory lanes
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        LoadVolumeParams();
        
        if (transform.childCount > 1)
        {
            sfxAudioSource = transform.GetChild(1).GetComponent<AudioSource>();
        }

        // Apply loaded sliders layout configurations safely
        UpdateSlidersUI();

        // Apply loaded parameters straight to the Audio Mixer upon startup
        MasterVolume(masterVol);
        MusicVolume(musicVol);
        SFXVolume(sfxVol);
        ToggleMute(isMuted);

        if (saveVolumes != null)
        {
            saveVolumes.onClick.AddListener(SaveVolumeParams);
        }

        // Initialize music clip processing rules for whatever scene was entered first
        EvaluateSceneBGM(SceneManager.GetActiveScene().name);
    }

    // ADDED: Unified Scene Loader Router Event Callback Trigger hook
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EvaluateSceneBGM(scene.name);
        
        UpdateSlidersUI();
    }

    // ADDED: Centralized background tracking logic analyzer processing
    private void EvaluateSceneBGM(string sceneName)
    {
        switch (sceneName)
        {
            case "MainMenu": 
                PlayBGM(mainKingdomMusic); 
                break;
            case "MainKingdom":
                PlayBGM(mainKingdomMusic);
                break;

            case "Level Select": 
                PlayBGM(levelSelectMusic);
                break;
        }
    }

    // ADDED: Track selector wrapper optimization routine
    private void PlayBGM(AudioClip trackClip)
    {
        if (trackClip == null || musicAudioSource == null) return;
        
        // Prevent jarring re-triggers if the correct track is already playing
        if (musicAudioSource.clip == trackClip && musicAudioSource.isPlaying) return;

        musicAudioSource.clip = trackClip;
        musicAudioSource.Play();
    }

    // ADDED: Extracted method helper to auto-find new settings layout objects on load
    private void UpdateSlidersUI()
    {
        if (masterSlider == null) masterSlider = GameObject.Find("masterSlider")?.GetComponent<Slider>();
        if (musicSlider == null) musicSlider = GameObject.Find("musicSlider")?.GetComponent<Slider>();
        if (sfxSlider == null) sfxSlider = GameObject.Find("sfxSlider")?.GetComponent<Slider>();

        if (masterSlider != null) { masterSlider.value = masterVol; masterSlider.onValueChanged.RemoveAllListeners(); masterSlider.onValueChanged.AddListener(MasterVolume); }
        if (musicSlider != null) { musicSlider.value = musicVol; musicSlider.onValueChanged.RemoveAllListeners(); musicSlider.onValueChanged.AddListener(MusicVolume); }
        if (sfxSlider != null) { sfxSlider.value = sfxVol; sfxSlider.onValueChanged.RemoveAllListeners(); sfxSlider.onValueChanged.AddListener(SFXVolume); }
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
            // Protect Log10 scale mapping metrics against absolute zeroes to avoid errors
            float dB = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
            audioMixer.SetFloat(MASTERVOLUME, dB);
        }
    }

    public void MusicVolume(float value)
    {
        musicVol = value;
        if (audioMixer != null)
        {
            float dB = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
            audioMixer.SetFloat(MUSICVOLUME, dB);
        }
    }

    public void SFXVolume(float value)
    {
        sfxVol = value;
        if (audioMixer != null)
        {
            float dB = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
            audioMixer.SetFloat(SFXVOLUME, dB);
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