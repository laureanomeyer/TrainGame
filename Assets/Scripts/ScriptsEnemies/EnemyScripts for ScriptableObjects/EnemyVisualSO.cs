using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Visual")]
public class EnemyVisualSO : ScriptableObject
{
    [Header("Visual prefab")]
    public GameObject visualPrefab;
    [Header("Animator overrides")]
    public AnimatorOverrideController cowboyAnimatorOverride;
    public AnimatorOverrideController mountAnimatorOverride;

}
