using UnityEngine;

[CreateAssetMenu(fileName = "New Station", menuName = "Train Game/Station")]
public class StationSO : ScriptableObject
{
    [SerializeField] private string stationName;

    public string StationName => stationName;
}