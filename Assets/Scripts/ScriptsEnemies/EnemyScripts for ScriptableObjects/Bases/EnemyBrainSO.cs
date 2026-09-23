using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Brain")]

public  class EnemyBrainSO : ScriptableObject
{

    public IWagon SetRandomTarget()
    {
        var wagonList = RunManager.Instance.ActiveWagons;

        int selectedWagon = Random.Range(0, wagonList.Count);

        return wagonList[selectedWagon];
    }

    public IWagon ReTarget(IWagon destroyedWagon, WagonType preference = WagonType.Random)
    {
        var wagonList = RunManager.Instance.ActiveWagons;
        int destroyedIndex = wagonList.IndexOf(destroyedWagon);

        int nextIndex = destroyedIndex + 1;
        if (nextIndex < wagonList.Count)
        {
            IWagon nextWagon = wagonList[nextIndex];
            if (IsValidTarget(nextWagon) && nextWagon != destroyedWagon)
                return GetPreference(preference);
        }

        int previousIndex = destroyedIndex - 1;
        foreach (IWagon wagon in wagonList)
        {
            IWagon previousWagon = wagonList[previousIndex];
            if (IsValidTarget(previousWagon) && previousWagon != destroyedWagon)
                return GetPreference(preference);
            previousIndex--;
        }
        return wagonList.Count > 0 && IsValidTarget(wagonList[0]) ? wagonList[0] : null;
    }

    public IWagon GetPreference(WagonType preference)
    {
        if (preference == WagonType.Random) return SetRandomTarget();
        foreach (var wagon in RunManager.Instance.ActiveWagons)
        {
            if (IsValidTarget(wagon) && wagon.WagonType == preference)
            {
                return wagon;
            }
        }
        return SetRandomTarget();
    }

    private bool IsValidTarget(IWagon wagon)
    {
        if (wagon == null || wagon.Head == null)
            return false;

        if (wagon is WagonBrain wagonBrain && wagonBrain.HPController != null)
            return !wagonBrain.HPController.IsBroken;

        return true;
    }

    public void Tick(Enemy enemy)
    {
        throw new System.NotImplementedException();
    }
}
