using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.Timeline.DirectorControlPlayable;

public class SettingsPanel : MonoBehaviour
{
    private const string MasterKey = "MasterVolume";
    private const string MusicKey = "MusicVolume";
    private const string SFXKey = "SFXVolume";
    private InputAction pauseAction;
    private bool isOpen = false;

    [Header("Panel")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button closeButton;

    [Header("Audio Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider SFXVolumeSlider;


    private void Awake()
    {
        pauseAction = InputSystem.actions.FindAction("Pause");

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

        if (pauseAction != null)
        {
            pauseAction.performed += Close;
        }
    }

    private void OnDestroy()
    {
        closeButton.onClick.RemoveListener(Close);

        masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
        SFXVolumeSlider.onValueChanged.RemoveListener(SetSFXVolume);

        if (pauseAction != null)
        {
            pauseAction.performed += Close;
        }

        PlayerPrefs.Save();
    }

    public void Open()
    {
        panel.SetActive(true);
        isOpen = true;
    }

    public void Close()
    {
        panel.SetActive(false);
        PlayerPrefs.Save();
        isOpen = false;
    }
    public void Close(InputAction.CallbackContext context)
    {
        if (!isOpen) return;

        panel.SetActive(false);
        PlayerPrefs.Save();

        isOpen = false;
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


}