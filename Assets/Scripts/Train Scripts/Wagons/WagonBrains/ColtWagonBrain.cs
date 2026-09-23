using System.Collections.Generic;
using UnityEngine;

public class ColtWagonBrain : WagonBrain
{
    private void Awake()
    {
        WagonType = WagonType.PassiveTorret;
    }
}