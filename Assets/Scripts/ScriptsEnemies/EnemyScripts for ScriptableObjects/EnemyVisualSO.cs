using UnityEngine;

[System.Serializable]
public class AnimationClipOverride
{
    public AnimationClip baseClip;
    public AnimationClip replacementClip;
}

[CreateAssetMenu(menuName = "Enemy/Visual")]
public class EnemyVisualSO : ScriptableObject
{
    [Header("Visual prefab")]
    public GameObject visualPrefab;
    public EnemyAnimationSO animations;
    [Header("Animator overrides")]
    public AnimatorOverrideController cowboyAnimatorOverride;
    public AnimatorOverrideController mountAnimatorOverride;
    public AnimationClipOverride[] cowboyClipOverrides = new AnimationClipOverride[0];
    public AnimationClipOverride[] mountClipOverrides = new AnimationClipOverride[0];
}
