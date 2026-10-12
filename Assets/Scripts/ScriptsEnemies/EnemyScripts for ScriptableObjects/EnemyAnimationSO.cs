using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Animation")]
public class EnemyAnimationSO : ScriptableObject
{
    [Header("Cowboy states")]
    public AnimationState positiveZIdle = new AnimationState("Left Layer.Bandido|L_CB_Revolver_Idle", true);
    public AnimationState positiveZAttack = new AnimationState("Left Layer.Bandido|L_CB_Revolver_Aim");
    public AnimationState positiveZDeath = new AnimationState("Left Layer.Bandido|L_CB_Revolver_Death");
    public AnimationState negativeZIdle = new AnimationState("Right Layer.Bandido|R_CB_Revolver_Idle", true);
    public AnimationState negativeZAttack = new AnimationState("Right Layer.Bandido|R_CB_Revolver_Aim");
    public AnimationState negativeZDeath = new AnimationState("Right Layer.Bandido|R_CB_Revolver_Death");
    [Range(0f, 1f)]
    public float firingNormalizedTime = 0.5f;

    [Header("Horse states")]
    public AnimationState horseIdle = new AnimationState("Base Layer.Bandido_Horse|Idle", true);

    private void OnValidate()
    {
        firingNormalizedTime = float.IsNaN(firingNormalizedTime)
            ? 0f
            : Mathf.Clamp01(firingNormalizedTime);
    }
}