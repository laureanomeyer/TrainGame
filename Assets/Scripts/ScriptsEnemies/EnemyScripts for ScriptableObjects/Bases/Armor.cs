public class Armor : ArcMotion
{
    private Enemy targetEnemy;
    public Enemy TargetEnemy => targetEnemy;

    public bool SetTarget(Enemy target)
    {
        if (target == null) return false;

        targetEnemy = target;
        BeginArcMotion(target.transform.position, speed, arcHeight);
        return true;
    }
}