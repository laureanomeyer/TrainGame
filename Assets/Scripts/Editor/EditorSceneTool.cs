#if UNITY_EDITOR
using UnityEngine;
using UnityEditor.Toolbars;
using UnityEditor;
using UnityEditor.SceneManagement;

public class EditorSceneTool 
{
    [MainToolbarElement("Scenes Button", defaultDockPosition = MainToolbarDockPosition.Right)]
    public static MainToolbarElement GetSceneButton()
    {
        Texture2D assetIcon = EditorGUIUtility.IconContent("SceneAsset Icon").image as Texture2D;
        return new MainToolbarButton(new MainToolbarContent("Scenes", assetIcon, "Opens the scene selection tool."), OnOpenScenesMenu);
    }

    private static void OnOpenScenesMenu()
    {
        string[] guids = AssetDatabase.FindAssets("t:SceneAsset");
        GenericMenu menu = new GenericMenu();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SceneAsset asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            menu.AddItem(new GUIContent(asset.name), false, () => { OpenScene(path); });
        }
        menu.ShowAsContext();
    }

    private static void OpenScene(string path)
    {
        EditorSceneManager.SaveOpenScenes();
        EditorSceneManager.OpenScene(path);
    }
}
#endif