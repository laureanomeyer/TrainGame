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

    public Transform SetSpecificTarget(int index, Enemy enemy)
    {
        return (enemy.TargetList[index].Head);
    }


    public void Tick(Enemy enemy)
    {
        throw new System.NotImplementedException();
    }
}
