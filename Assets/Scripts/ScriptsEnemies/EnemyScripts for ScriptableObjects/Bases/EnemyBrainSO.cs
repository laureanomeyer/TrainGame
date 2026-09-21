using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Brain")]

public  class EnemyBrainSO : ScriptableObject
{
    public void Begin(Enemy enemy)
    {

    }

    public IWagon SetRandomTarget()
    {
        return RunManager.Instance.ActiveWagons[Random.Range(0 , RunManager.Instance.ActiveWagons.Count)];
    }

    public IWagon ReTarget(Enemy enemy)
    {
        var WagonList = RunManager.Instance.ActiveWagons;

        int wagonIndex = WagonList.IndexOf(enemy.TargetWagon);
        wagonIndex = Random.Range(0, 2) *2-1 + wagonIndex; if (wagonIndex < 0) wagonIndex = 0; if (wagonIndex > WagonList.Count) wagonIndex = WagonList.Count - 1;

        if (WagonList[wagonIndex] != null)
        {
            return WagonList[wagonIndex];
        }
        else
        {
            return ReTarget(enemy);
        }
    }

    public void Tick(Enemy enemy)
    {
        throw new System.NotImplementedException();
    }
}
