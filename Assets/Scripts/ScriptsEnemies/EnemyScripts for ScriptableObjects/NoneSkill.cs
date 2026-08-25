using System;
using UnityEngine;

[Serializable]
public class NoneSkill : EnemySkill
{

    public override void Play(Enemy enemy)
    {
        return;
    }

    public override void Stop(Enemy enemy)
    {
        return;
    }
}