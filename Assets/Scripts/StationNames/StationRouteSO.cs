using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Station Route", menuName = "Train Game/Station Route")]
public class StationRouteSO : ScriptableObject
{
    [SerializeField] private List<StationSO> stations = new List<StationSO>();

    public string GetStationNameByLevel(int level)
    {
        if (stations == null || stations.Count == 0)
        {
            return "Estación desconocida";
        }

        int index = Mathf.Clamp(level - 1, 0, stations.Count - 1);

        if (stations[index] == null)
        {
            return "Estación desconocida";
        }

        return stations[index].StationName;
    }

    public int StationCount => stations.Count;
}