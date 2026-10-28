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

    [Header("Encuadre de cámara (opcional)")]
    [Tooltip("La cámara viaja a una pose fija respecto del Entrance y lo mira. Si está apagado, queda la cámara de gameplay con paneo.")]
    public bool useCustomFraming = false;
    [Tooltip("Posición de la cámara relativa al Entrance, en ejes del mundo (X = hacia donde avanza el tren).")]
    public Vector3 cameraOffsetFromEntrance = new(-18f, 34f, -22f);
    [Tooltip("Punto al que mira la cámara, relativo al Entrance. X negativo deja la boca hacia la derecha de la pantalla.")]
    public Vector3 lookAtOffsetFromEntrance = new(-10f, 0f, -3f);

    public LevelEndTransitionConfig() { }

    public LevelEndTransitionConfig(LevelEndStyle style, TileBiome continueWith, bool useCustomFraming = false)
    {
        this.style = style;
        this.continueWith = continueWith;
        this.useCustomFraming = useCustomFraming;
    }
}