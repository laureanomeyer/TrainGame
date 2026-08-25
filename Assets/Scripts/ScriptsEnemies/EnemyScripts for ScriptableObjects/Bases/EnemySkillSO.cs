using UnityEngine;

public enum SkillType
{
    None,
    Dynamite,
}

[CreateAssetMenu(fileName = "EnemySkillSO", menuName = "Enemy/Skill")]
public class EnemySkillSO : ScriptableObject
{
    public SkillType skillType;

    [SerializeReference] private EnemySkill skill;

    public float Cooldown => skill.cooldown;

    public void Play(Enemy enemy) => skill?.Play(enemy);
    public void Stop(Enemy enemy) => skill?.Stop(enemy);

#if UNITY_EDITOR
    private void OnValidate()
    {
        switch (skillType)
        {
            case SkillType.Dynamite:
                if (!(skill is DynamiteSkill))
                    skill = new DynamiteSkill();
                break;

            case SkillType.None:
                if (!(skill is NoneSkill))
                    skill = new NoneSkill();
                break;
        }
    }
#endif
}