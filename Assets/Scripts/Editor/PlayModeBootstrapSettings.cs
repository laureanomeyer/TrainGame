using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayModeBootstrapSettings", menuName = "Bootstrap/Play Mode Bootstrap Settings")]
public class PlayModeBootstrapSettings : ScriptableObject
{
    [Tooltip("Activa o desactiva el auto-preload al entrar en Play Mode.")]
    public bool enabled = true;

    [Tooltip("Prefabs que se instancian automáticamente al entrar en Play, si la escena activa no está excluida.")]
    public List<GameObject> prefabsToPreload = new List<GameObject>();

    [Tooltip("Escenas donde NO se debe instanciar nada (ej: tu Main Menu, donde estos objetos ya existen de forma normal).")]
    public List<SceneAsset> excludedScenes = new List<SceneAsset>();
}