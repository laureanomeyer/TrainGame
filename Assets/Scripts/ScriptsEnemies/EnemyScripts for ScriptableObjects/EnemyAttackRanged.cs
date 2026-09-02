using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Attack/Ranged")]
public class EnemyAttackRanged : EnemyAttackSO
{
    public override void Attack(Enemy enemy)
    {
        if (enemy.Target == null) return;
        if (!enemy.CanAttack) return;

        float dist = Vector3.Distance(enemy.transform.position, enemy.Target.position);

        if (dist <= enemy.Range + 5)
        {
            bool isOnScreen = CameraView.IsInsideCamera(enemy.transform.position, enemy.Cam);
            enemy.Weapon.Execute(enemy.Target, enemy.Damage);
            enemy.ResetAttackCooldown(enemy.Cooldown);

            AudioManager.Instance.PlayOnScreen("SFXEnemyShot", isOnScreen);

        }
    }
}