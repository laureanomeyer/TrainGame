using System.Collections.Generic;
using UnityEngine;

namespace GGG.RTO.Wagons
{
    public sealed class EastWagonBrain : WagonBrain
    {
        [SerializeField] private float hpBonus;
        public override IEnumerable<StatModifier> GetModifiers()
        {
            yield return new StatModifier(StatType.MaxHp, hpBonus, ModifierType.Additive, this);
        }
    }
}