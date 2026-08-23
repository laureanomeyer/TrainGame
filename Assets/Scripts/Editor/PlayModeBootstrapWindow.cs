using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayModeBootstrapWindow : EditorWindow
{
    private const string SettingsPath = "Assets/Editor/PlayModeBootstrapSettings.asset";

    private static PlayModeBootstrapSettings settings;
    private SerializedObject serializedSettings;

    [MenuItem("Tools/Bootstrap/Preload Settings")]
    public static void OpenWindow()
    {
        GetWindow<PlayModeBootstrapWindow>("Bootstrap Preload");
    }

    private void OnEnable()
    {
        LoadOrCreateSettings();
        serializedSettings = new SerializedObject(settings);
    }

    private static void LoadOrCreateSettings()
    {
        settings = AssetDatabase.LoadAssetAtPath<PlayModeBootstrapSettings>(SettingsPath);

        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<PlayModeBootstrapSettings>();

            if (!AssetDatabase.IsValidFolder("Assets/Editor"))
                AssetDatabase.CreateFolder("Assets", "Editor");

            AssetDatabase.CreateAsset(settings, SettingsPath);
            AssetDatabase.SaveAssets();
        }
    }

    private void OnGUI()
    {
        if (settings == null)
        {
            LoadOrCreateSettings();
            serializedSettings = new SerializedObject(settings);
        }

        serializedSettings.Update();

        EditorGUILayout.LabelField("Play Mode Bootstrap", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Los prefabs de la lista se instancian automáticamente al entrar en Play " +
            "desde cualquier escena (salvo las excluidas). Útil para probar niveles sin " +
            "pasar por el menú principal ni las pantallas de carga.",
            MessageType.Info);

        EditorGUILayout.PropertyField(serializedSettings.FindProperty("enabled"));
        EditorGUILayout.PropertyField(serializedSettings.FindProperty("prefabsToPreload"), true);
        EditorGUILayout.PropertyField(serializedSettings.FindProperty("excludedScenes"), true);

        serializedSettings.ApplyModifiedProperties();
    }

    [InitializeOnLoadMethod]
    private static void Init()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode)
            return;

        LoadOrCreateSettings();

        if (!settings.enabled || settings.prefabsToPreload.Count == 0)
            return;

        string activeScenePath = SceneManager.GetActiveScene().path;

        bool isExcluded = settings.excludedScenes
            .Where(s => s != null)
            .Select(AssetDatabase.GetAssetPath)
            .Any(path => path == activeScenePath);

        if (isExcluded)
            return;

        foreach (GameObject prefab in settings.prefabsToPreload)
        {
            if (prefab == null) continue;

            if (GameObject.Find(prefab.name) != null)
                continue;

            GameObject instance = Object.Instantiate(prefab);
            instance.name = prefab.name;
        }
    }
}