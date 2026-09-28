using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WagonUpgradePath", menuName = "Store/Wagon Upgrade Path")]
public class WagonUpgradePathSO : ScriptableObject
{
    [Serializable]
    public struct WagonLevel
    {
        [Tooltip("SO de este nivel (shopModel = modelo del display, Wagon = prefab de gameplay)")]
        public WagonInStockSO wagon;

        [Tooltip("Costo para pasar DE este nivel al siguiente. Se ignora en el último nivel.")]
        public float upgradeCost;
    }

    [Tooltip("Índice 0 = Nivel 1, 1 = Nivel 2, 2 = Nivel 3")]
    public WagonLevel[] levels;
}