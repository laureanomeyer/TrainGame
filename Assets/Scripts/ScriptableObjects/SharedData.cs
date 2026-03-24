using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SharedData", menuName = "Scriptable Objects/SharedData")]
public class SharedData : ScriptableObject
{
    public float speed;
    public Transform tailPosition;
    public List<IWagon> wagonList;
}
