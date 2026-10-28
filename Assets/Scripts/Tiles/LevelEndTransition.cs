using System;
using UnityEngine;

public enum LevelEndTransition
{
    WoodenThreshold,
    DesertToDesert,
    DesertToGrass,
    GrassToGrass,
    GrassToCave,
    CaveToCave
}

public enum LevelEndStyle
{
    [Tooltip("El mapa frena, el tren entra al túnel y la cámara queda fija. No vienen más tiles.")]
    EnterTunnel,
    [Tooltip("El mapa sigue andando, el tren cruza el umbral y siguen viniendo tiles del bioma siguiente.")]
    PassThrough
}

public enum TileBiome
{
    Desert,
    Grass,
    Cave
}

[Serializable]
public class LevelTransitionEntry
{
    [Min(1)] public int level = 1;
    public LevelEndTransition transition = LevelEndTransition.WoodenThreshold;
}

[Serializable]
public class LevelEndTransitionConfig
{
    [Tooltip("Prefab con MapTile + LevelEndTile.")]
    public LevelEndTile endTile;
    public LevelEndStyle style = LevelEndStyle.PassThrough;
    [Tooltip("Bioma de los tiles que siguen viniendo después del umbral. Solo se usa en PassThrough.")]
    public TileBiome continueWith = TileBiome.Desert;

    public LevelEndTransitionConfig() { }

    public LevelEndTransitionConfig(LevelEndStyle style, TileBiome continueWith)
    {
        this.style = style;
        this.continueWith = continueWith;
    }
}