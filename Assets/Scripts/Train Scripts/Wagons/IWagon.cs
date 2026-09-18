using UnityEngine;
public interface IWagon
{
    Transform Head { get;}
    Transform Tail { get;}

    void Move() { }
}

