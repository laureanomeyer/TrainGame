using UnityEngine;
public interface IWagon
{
    Transform Head { get; }
    Transform Tail { get; }
    Vector3 Middle => (Head.position + Tail.position) * 0.5f;

    WagonType WagonType { get; }

    void Move() { }
}

