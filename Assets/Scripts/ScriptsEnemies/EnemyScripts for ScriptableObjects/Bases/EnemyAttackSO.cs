using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Attack")]

public class EnemyAttackSO : ScriptableObject
{
    public void Attack(Enemy enemy)
    {
        RangedAttack(enemy);
    }

    public void Skill(Enemy enemy)
    {
        if (enemy.Target == null) return;
        if (!enemy.CanSkill) return;

        if (enemy.CanSkill && enemy.Skill != null)
        {
            enemy.Skill.Play(enemy);
            enemy.ResetSkillCooldown(enemy.Skill.Cooldown);
        }
    }

    private void RangedAttack(Enemy enemy)
    {
        if (enemy.Target == null) return;
        if (!enemy.CanAttack) return;

        float dist = Vector3.Distance(enemy.transform.position, enemy.Target.position);

        if (dist <= enemy.Range + 5)
        {
            if (enemy.TargetWagon == null) return;

            bool isOnScreen = CameraView.IsInsideCamera(enemy.transform.position, enemy.Cam);
            enemy.PlayAttackAnimation();
            enemy.Weapon.Execute(enemy.TargetWagon, enemy.Damage);
            enemy.ResetAttackCooldown(enemy.Cooldown);
            PlayAudio(isOnScreen);

        }
    }

    private void PlayAudio(bool isOnScreen)
    {
        AudioManager.Instance.PlayOnScreen("SFXEnemyShot", isOnScreen);
    }

}
