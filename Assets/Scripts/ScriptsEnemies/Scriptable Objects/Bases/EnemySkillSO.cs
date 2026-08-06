using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySkillSO", menuName = "Enemy/Skill")]
public class EnemySkillSO : ScriptableObject
{
    public EnemySkill skill;
    protected float cooldown;
    protected float timer;

    public bool CanUse => timer <= 0f;

    public virtual void Init(Enemy enemy) { }
    public void Play(Enemy enemy)
    {
        skill.Play(enemy);
    }

    public void Stop(Enemy enemy)
    {
        skill.Stop(enemy);
    }

    protected void ResetCooldown() => timer = cooldown;

}
