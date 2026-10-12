using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Visual")]
public class EnemyVisualSO : ScriptableObject
{
    [Header("Visual prefab")]
    public GameObject visualPrefab;

    [Header("Shared runtime controller")]
    public RuntimeAnimatorController runtimeAnimatorController;

    [Header("Optional fallback mapping")]
    public EnemyAnimationSO animation;

    [Header("Optional direct material override")]
    public Material[] materials;
}
