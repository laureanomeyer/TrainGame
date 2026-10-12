using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Animation")]
public class EnemyAnimationSO : ScriptableObject
{
    [Header("Cowboy states")]
    public string positiveZIdle = "Recluta|L_CB_Revolver_Idle";
    public string positiveZAttack = "Recluta|L_CB_Revolver_Aim";
    public string positiveZDeath = "Recluta|L_CB_Revolver_Death";
    public string negativeZIdle = "Recluta|R_CB_Revolver_Idle";
    public string negativeZAttack = "Recluta|R_CB_Revolver_Aim";
    public string negativeZDeath = "Recluta|R_CB_Revolver_Death";
    public float attackDuration = 0.8f;
    public float deathDuration = 1f;

    [Header("Horse states")]
    public string horseIdle = "Recluta_Horse|Idle";
}