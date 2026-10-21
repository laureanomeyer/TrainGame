using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    private const string MasterKey = "MasterVolume";
    private const string MusicKey = "MusicVolume";
    private const string SFXKey = "SFXVolume";

    [Header("Panel")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button closeButton;

    [Header("Audio Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider SFXVolumeSlider;

    private void Start()
    {
        closeButton.onClick.AddListener(Close);

        float savedMaster = PlayerPrefs.GetFloat(MasterKey, 0.2f);
        float savedMusic = PlayerPrefs.GetFloat(MusicKey, 1f);
        float savedSFX = PlayerPrefs.GetFloat(SFXKey, 1f);

        masterVolumeSlider.value = savedMaster;
        musicVolumeSlider.value = savedMusic;
        SFXVolumeSlider.value = savedSFX;

        ApplyMasterVolume(savedMaster);
        ApplyMusicVolume(savedMusic);
        ApplySFXVolume(savedSFX);

        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        SFXVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        panel.SetActive(false);
    }

    public void Open()
    {
        panel.SetActive(true);
    }

    public void Close()
    {
        panel.SetActive(false);
        PlayerPrefs.Save();
    }

    private void SetMasterVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        ApplyMasterVolume(volume);
        PlayerPrefs.SetFloat(MasterKey, volume);
    }

    private void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        ApplyMusicVolume(volume);
        PlayerPrefs.SetFloat(MusicKey, volume);
    }

    private void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        ApplySFXVolume(volume);
        PlayerPrefs.SetFloat(SFXKey, volume);
    }

    private void ApplyMasterVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    private void ApplyMusicVolume(float volume)
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.SetVolume(volume);
    }

    private void ApplySFXVolume(float volume)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(volume);
    }

    private void OnDestroy()
    {
        closeButton.onClick.RemoveListener(Close);

        masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
        SFXVolumeSlider.onValueChanged.RemoveListener(SetSFXVolume);

        PlayerPrefs.Save();
    }
}