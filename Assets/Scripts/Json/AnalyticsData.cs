using System.Collections.Generic;
using UnityEngine;

// =========================================================
//  JSON GLOBAL: DATA
// =========================================================

/// <summary>
/// Datos "crudos" globales que el juego usa y carga/guarda.
/// </summary>
[System.Serializable]
public class GlobalAnalyticsData
{
    public int playerDefeatData = 0;
    public int playerWinData = 0;
}

/// <summary>
/// Texto legible para personas (JSON global).
/// </summary>
[System.Serializable]
public class GlobalResumen
{
    // JsonUtility no permite espacios en los nombres de campos,
    // por eso el texto legible se arma como valor del campo.
    public string playerDefeatText = "Total de derrotas: 0";
    public string playerWinText = "Total de victorias: 0";
}


// =========================================================
//  JSON DE USUARIOS: DATA
// =========================================================

/// <summary>
/// Información de UN usuario.
/// Todas las variables empiezan en 0 al crear un usuario nuevo;
/// si el usuario ya existe en el JSON, se cargan sus valores guardados.
/// </summary>
[System.Serializable]
public class UserData
{
    //Nombre de usuario
    public int userId = 0;

    //Muestra de informacion del usuario 
    public int runsPlay = 0;
    public int wins = 0;
    public int defeats = 0;
}

/// <summary>
/// Datos "crudos" de todos los usuarios.
/// </summary>
[System.Serializable]
public class UserAnalyticsData
{
    public List<UserData> users = new List<UserData>();
}

/// <summary>
/// Texto legible de UN usuario: un string por variable.
/// </summary>
[System.Serializable]
public class UserSummaryEntry
{
    //Nombre de usuario
    public string userText = "Usuario 0";

    //Muestra de informacion del usuario 
    public string runPlayText = "Cantidad de partidas jugadas: 0";
    public string defeatsText = "Cantidad de derrotas: 0";
    public string winsText = "Cantidad de victorias: 0";

    // Línea decorativa para separar visualmente a un usuario del siguiente.
    public string separatorText = "-----------------------------------------------------";
}

/// <summary>
/// Texto legible para personas (JSON de usuarios).
/// </summary>
[System.Serializable]
public class UserResumen
{
    public string totalUsersText = "Total de usuarios registrados: 0";
    public List<UserSummaryEntry> usersSummary = new List<UserSummaryEntry>();
}

