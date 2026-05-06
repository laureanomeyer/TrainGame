using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    [SerializeField] private TransitionEffect transitionEffect;

    private readonly Dictionary<string, string> loadedScenesBySlot = new();

    private bool isBusy;

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

    public SceneTransitionPlan NewTransition()
    {
        return new SceneTransitionPlan(this);
    }

    public Coroutine ExecutePlan(SceneTransitionPlan plan)
    {
        if (isBusy)
        {
            Debug.LogWarning("SceneController is busy. Transition ignored.");
            return null;
        }

        return StartCoroutine(ExecutePlanRoutine(plan));
    }

    private IEnumerator ExecutePlanRoutine(SceneTransitionPlan plan)
    {
        isBusy = true;

        if (plan.UseOverlay && transitionEffect != null)
        {
            yield return transitionEffect.FadeIn();
        }

        foreach (string slotKey in plan.SlotsToUnload)
        {
            yield return UnloadSlotRoutine(slotKey);
        }

        if (plan.ClearUnusedAssets)
        {
            yield return Resources.UnloadUnusedAssets();
        }

        foreach (KeyValuePair<string, string> sceneToLoad in plan.ScenesToLoad)
        {
            string slotKey = sceneToLoad.Key;
            string sceneName = sceneToLoad.Value;

            if (loadedScenesBySlot.ContainsKey(slotKey))
            {
                yield return UnloadSlotRoutine(slotKey);
            }

            yield return LoadSceneRoutine(slotKey, sceneName, plan.ActiveSceneName == sceneName);
        }

        if (plan.UseOverlay && transitionEffect != null)
        {
            yield return transitionEffect.FadeOut();
        }

        isBusy = false;
    }

    private IEnumerator LoadSceneRoutine(string slotKey, string sceneName, bool setActive)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        if (operation == null)
        {
            Debug.LogError($"Could not load scene: {sceneName}. Check Build Settings.");
            yield break;
        }

        while (!operation.isDone)
        {
            yield return null;
        }

        Scene loadedScene = SceneManager.GetSceneByName(sceneName);

        if (!loadedScene.IsValid())
        {
            Debug.LogError($"Loaded scene is not valid: {sceneName}");
            yield break;
        }

        loadedScenesBySlot[slotKey] = sceneName;

        if (setActive)
        {
            SceneManager.SetActiveScene(loadedScene);
        }
    }

    private IEnumerator UnloadSlotRoutine(string slotKey)
    {
        if (!loadedScenesBySlot.TryGetValue(slotKey, out string sceneName))
        {
            yield break;
        }

        Scene scene = SceneManager.GetSceneByName(sceneName);

        if (!scene.IsValid() || !scene.isLoaded)
        {
            loadedScenesBySlot.Remove(slotKey);
            yield break;
        }

        AsyncOperation operation = SceneManager.UnloadSceneAsync(scene);

        if (operation == null)
        {
            Debug.LogWarning($"Could not unload scene: {sceneName}");
            yield break;
        }

        while (!operation.isDone)
        {
            yield return null;
        }

        loadedScenesBySlot.Remove(slotKey);
    }
}