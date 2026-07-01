using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Playlist (nombres deben coincidir con Sound.name en AudioManager)")]
    [SerializeField] private int trackCount = 9; // cuántas canciones OST hay (OST1..OST5, por ej.)

    [Header("Pitch por estado")]
    [SerializeField] private float shopPitch = 1f;
    [SerializeField] private float combatPitch = 1.25f;
    [SerializeField] private float pitchTransitionSpeed = 1.5f;

    [Header("Volumen por estado")]
    [SerializeField] private float shopVolume = 0.6f;
    [SerializeField] private float combatVolume = 1f;
    [SerializeField] private float volumeTransitionSpeed = 1f;

    private int currentTrackIndex = 1; // arranca en OST1
    private float targetPitch = 1f;
    private float targetVolume = 1f;
    private string CurrentTrackName => $"OST{currentTrackIndex}";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        targetPitch = shopPitch;
        targetVolume = shopVolume;
    }

    private void Start()
    {
        AudioManager.Instance.Play(CurrentTrackName);
    }

    private void Update()
    {
        UpdateTargets();

        AudioSource src = AudioManager.Instance.GetSource(CurrentTrackName);
        if (src != null)
        {
            src.pitch = Mathf.MoveTowards(src.pitch, targetPitch, pitchTransitionSpeed * Time.deltaTime);
            src.volume = Mathf.MoveTowards(src.volume, targetVolume, volumeTransitionSpeed * Time.deltaTime);
        }

        if (!AudioManager.Instance.IsPlaying(CurrentTrackName))
        {
            currentTrackIndex = currentTrackIndex % trackCount + 1;
            AudioManager.Instance.Play(CurrentTrackName);
            AudioManager.Instance.SetPitch(CurrentTrackName, targetPitch);
            AudioManager.Instance.SetVolume(CurrentTrackName, targetVolume);
        }
    }

    private void UpdateTargets()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.IsInCombat)
        {
            targetPitch = combatPitch;
            targetVolume = combatVolume;
        }
        else if (GameManager.Instance.IsInShop)
        {
            targetPitch = shopPitch;
            targetVolume = shopVolume;
        }
    }
}