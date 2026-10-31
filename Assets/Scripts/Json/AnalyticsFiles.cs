using UnityEngine;

// =========================================================
//  JSON GLOBAL: FILE
// =========================================================

/// <summary>
/// Estructura completa del archivo JSON global.
/// - Datos: lo que lee el código del juego.
/// - Resumen: lo que lee una persona al abrir el archivo.
/// </summary>
[System.Serializable]
public class GlobalAnalyticsFile
{
    public GlobalAnalyticsData Datos = new GlobalAnalyticsData();
    public GlobalResumen Resumen = new GlobalResumen();
}

// =========================================================
//  JSON GLOBAL: FILE
// =========================================================

/// <summary>
/// Estructura completa del archivo JSON de usuarios.
/// </summary>
[System.Serializable]
public class UserAnalyticsFile
{
    public UserAnalyticsData Datos = new UserAnalyticsData();
    public UserResumen Resumen = new UserResumen();
}
