using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WagonUpgradePath", menuName = "Store/Wagon Upgrade Path")]
public class WagonUpgradePathSO : ScriptableObject
{
    [Serializable]
    public struct WagonLevel
    {
        [Tooltip("SO de este nivel. Su Price es lo que cuesta mejorar HACIA este nivel (en el nivel 1, es el precio de compra).")]
        public WagonInStockSO wagon;
    }

    [Tooltip("Índice 0 = Nivel 1, 1 = Nivel 2, 2 = Nivel 3")]
    public WagonLevel[] levels;
}