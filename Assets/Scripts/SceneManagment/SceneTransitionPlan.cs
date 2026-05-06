using System.Collections.Generic;
using UnityEngine;

public class SceneTransitionPlan
{
    public Dictionary<string, string> ScenesToLoad { get; } = new();
    public List<string> SlotsToUnload { get; } = new();

    public bool UseOverlay { get; private set; }
    public bool ClearUnusedAssets { get; private set; }
    public string ActiveSceneName { get; private set; }

    private readonly SceneController sceneController;

    public SceneTransitionPlan(SceneController sceneController)
    {
        this.sceneController = sceneController;
    }

    public SceneTransitionPlan Load(string slotKey, string sceneName, bool setActive = false)
    {
        ScenesToLoad[slotKey] = sceneName;

        if (setActive)
        {
            ActiveSceneName = sceneName;
        }

        return this;
    }

    public SceneTransitionPlan Unload(string slotKey)
    {
        if (!SlotsToUnload.Contains(slotKey))
        {
            SlotsToUnload.Add(slotKey);
        }

        return this;
    }

    public SceneTransitionPlan WithOverlay()
    {
        UseOverlay = true;
        return this;
    }

    public SceneTransitionPlan WithClearUnusedAssets()
    {
        ClearUnusedAssets = true;
        return this;
    }

    public Coroutine Perform()
    {
        return sceneController.ExecutePlan(this);
    }
}