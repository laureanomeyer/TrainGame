using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Animation")]
public class EnemyAnimationSO : ScriptableObject
{
    [Header("Cowboy states")]
    public string positiveZIdle = "Cowboy_1|L_Idle";
    public string positiveZAttack = "Cowboy_1|L_Aim";
    public string positiveZDeath = "Cowboy_1|L_Death";
    public string negativeZIdle = "Cowboy_1|R_Idle";
    public string negativeZAttack = "Cowboy_1|R_Aim 0";
    public string negativeZDeath = "Cowboy_1|R_Death";
    public float attackDuration = 0.8f;
    public float deathDuration = 1f;

    [Header("Horse states")]
    public string horseIdle = "Horse|Idle";
}