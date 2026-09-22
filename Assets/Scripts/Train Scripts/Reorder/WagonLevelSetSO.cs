using System;
using UnityEngine;

// Data de niveles de un tipo de vagón. Un WagonInStockSO que no tenga
// asignado ninguno de estos simplemente no puede mejorarse (vagones
// "Costo-Recompensa" según el documento de diseño).
[CreateAssetMenu(fileName = "WagonLevelSet", menuName = "Store/Wagon Level Set")]
public class WagonLevelSetSO : ScriptableObject
{
    [Serializable]
    public class LevelEntry
    {
        [Min(1)] public int level = 2;

        [Tooltip("Prefab que instancia TrainManager en combate para este nivel.")]
        public GameObject gameplayPrefab;

        [Tooltip("Prefab visual que se muestra en la tienda (DisplayTrain) para este nivel.")]
        public GameObject shopModel;

        [Tooltip("Costo en oro para subir A este nivel desde el anterior.")]
        public float upgradeCost;
    }

    [Tooltip("No incluyas una entrada de nivel 1 acá: el nivel 1 lo define el WagonInStockSO. " +
             "Cargá solo las mejoras disponibles (nivel 2, y nivel 3 si corresponde).")]
    public LevelEntry[] levels;

    public LevelEntry GetLevel(int level)
    {
        foreach (var entry in levels)
            if (entry.level == level) return entry;
        return null;
    }
}