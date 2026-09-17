using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Brain")]

public  class EnemyBrainSO : ScriptableObject
{
    public void Begin(Enemy enemy)
    {
        
    }

    public Transform SetTarget(Enemy enemy)
    {
        return enemy.TargetList[Random.Range(0 , enemy.TargetList.Count)].Head;
    }

    public void Tick(Enemy enemy)
    {
        throw new System.NotImplementedException();
    }
}
