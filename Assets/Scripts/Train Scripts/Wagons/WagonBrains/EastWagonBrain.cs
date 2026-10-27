using System.Collections.Generic;
using UnityEngine;

namespace GGG.RTO.Wagons
{
    public sealed class EastWagonBrain : WagonBrain
    {
        [SerializeField] private float hpBonus;

        private void Awake()
        {
            WagonType = WagonType.Passive;
        }

        public override IEnumerable<List<StatModifier>> GetModifiers()
        {
            List<StatModifier> mods = new List<StatModifier>(); ;
            mods.Add(new StatModifier(StatType.MaxHp, hpBonus, ModifierType.Additive, this));
            yield return mods;
        }
    }
}