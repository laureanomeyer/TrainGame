using UnityEngine;
using UnityEngine.UI;

public class MainMenuButtons : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button closeSettingsButton;

    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Audio Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider SFXVolumeSlider;

    private void Start()
    {
        startButton.onClick.AddListener(GameManager.Instance.StartNewSession);
        quitButton.onClick.AddListener(Quit);

        settingsButton.onClick.AddListener(OpenSettings);
        closeSettingsButton.onClick.AddListener(CloseSettings);

        float savedMasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        masterVolumeSlider.value = savedMasterVolume;
        musicVolumeSlider.value = savedMusicVolume;
        SFXVolumeSlider.value = savedSFXVolume;


        SetMasterVolume(savedMasterVolume);
        SetMusicVolume(savedMusicVolume);
        SetSFXVolume(savedSFXVolume);

        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        SFXVolumeSlider.onValueChanged.AddListener(SetSFXVolume);


        settingsPanel.SetActive(false);
    }

    private void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    private void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    private void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;

        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }

    private void SetMusicVolume(float volume)
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetVolume(volume);
        }
    }

    private void SetSFXVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(volume);
        }
    }

    private void OnDestroy()
    {
        startButton.onClick.RemoveAllListeners();
        quitButton.onClick.RemoveAllListeners();
        settingsButton.onClick.RemoveAllListeners();
        closeSettingsButton.onClick.RemoveAllListeners();

        masterVolumeSlider.onValueChanged.RemoveAllListeners();
        musicVolumeSlider.onValueChanged.RemoveAllListeners();
    }

    private void Quit()
    {
        Application.Quit();
    }
}