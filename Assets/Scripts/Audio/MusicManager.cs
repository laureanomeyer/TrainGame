using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Playlist (nombres deben coincidir con Sound.name en AudioManager)")]
    [SerializeField] private int trackCount = 9; // cuántas canciones OST hay (OST1..OST5, por ej.)

    [Header("Pitch por estado")]
    [SerializeField] private float shopPitch = 1f;
    [SerializeField] private float combatPitch = 1f;
    [SerializeField] private float pitchTransitionSpeed = 1.5f;

    [Header("Volumen por estado")]
    [SerializeField] private float musicVolume = 1f;
    [SerializeField] private float volumeTransitionSpeed = 1f;

    [Header("Bandas Sonoras")]
    [SerializeField] Sound[] storeOST;
    [SerializeField] Sound[] gameplayOST;
    [SerializeField] Sound[] menuOST;

    [Header("Bandas Sonoras")]
    [SerializeField] Sound[] ambientSound;

    private Sound currentOST;

   // private string CurrentTrackName => $"OST{currentTrackIndex}";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        foreach (var s in storeOST) AudioManager.Instance.InitializeSound(s);
        foreach (var s in gameplayOST) AudioManager.Instance.InitializeSound(s);
        foreach (var s in menuOST) AudioManager.Instance.InitializeSound(s);

        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);

        SetMenuMusic();
    }


    public void SetStoreMusic()
    {
        AudioManager.Instance.Stop(currentOST);

        currentOST = storeOST[Random.Range(0, storeOST.Length)];

        AudioManager.Instance.Play(currentOST);
        currentOST.source.volume = musicVolume;

        Debug.Log("Store");
    }

    public void SetGameplayMusic()
    {
        AudioManager.Instance.Stop(currentOST);

        currentOST = gameplayOST[Random.Range(0, gameplayOST.Length)];

        AudioManager.Instance.Play(currentOST);
        currentOST.source.volume = musicVolume;

        Debug.Log("Gameplay");
    }

    public void SetMenuMusic()
    {
        AudioManager.Instance.Stop(currentOST);

        currentOST = menuOST[Random.Range(0, menuOST.Length)];

        AudioManager.Instance.Play(currentOST);
        currentOST.source.volume = musicVolume;

        Debug.Log("Menu");
    }

    public void SetVolume(float volume)
    {
        musicVolume = volume;

        if (currentOST != null && currentOST.source != null)
        {
            currentOST.source.volume = musicVolume;
        }

        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();
    }

    public void SetAmbientSound(float volume) 
    {
        AudioManager.Instance.Play("TrainPassing");
    }

    /*  private void UpdateTargets()
      {
          if (GameManager.Instance == null) return;

          if (GameManager.Instance.IsInCombat)
          {
              targetPitch = combatPitch;
              targetVolume = musicVolume;
          }
          else if (GameManager.Instance.IsInShop)
          {
              targetPitch = shopPitch;
              targetVolume = musicVolume;
          }
      }*/
}