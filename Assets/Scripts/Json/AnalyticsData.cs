using System.Collections.Generic;

/// <summary>
/// Un par índice-cantidad (reemplaza a una entrada de Dictionary).
/// </summary>
[System.Serializable]
public class WagonCountEntry
{
    public int wagonIndex = 0;
    public int count = 0;
}

/// <summary>
/// Un par índice-cantidad (reemplaza a una entrada de Dictionary).
/// </summary>
[System.Serializable]
public class RunResultsEntry
{
    public int runPlay = 0;
    public bool result;
}

/// <summary>
/// Un par estación-derrotas (reemplaza a una entrada de Dictionary<string, int>).
/// </summary>
[System.Serializable]
public class StationDefeatEntry
{
    public string stationName = "";
    public int defeats = 0;
}

/// <summary>
/// Un par enemigo-muertes (reemplaza a Dictionary<string, int>).
/// </summary>
[System.Serializable]
public class EnemyKillEntry
{
    public string enemyName = "";
    public int kills = 0;
}

/// <summary>
/// Enemigos muertos en UNA run (reemplaza a una entrada de Dictionary<int, Dictionary<string, int>>).
/// </summary>
[System.Serializable]
public class RunEnemyKillsEntry
{
    public int runPlay = 0;
    public List<EnemyKillEntry> enemies = new List<EnemyKillEntry>();
}

/// <summary>
/// Cantidad de interacciones con la UI de WagonManager.
/// Todas las variables arrancan en 0.
/// </summary>
[System.Serializable]
public struct WagonManagerInteraction
{
    public int managerClick;
    public int moveClick;
    public int sellClick;
    public int upgradesClick;
}

/// <summary>
/// Interacciones con WagonManager en UNA run.
/// </summary>
[System.Serializable]
public class RunWagonManagerEntry
{
    public int runPlay = 0;
    public WagonManagerInteraction interactions;
}

/// <summary>
/// Mejoras de la locomotora. Todas las variables arrancan en 0.
/// </summary>
[System.Serializable]
public struct LocomotiveUpgrades
{
    public int maxHPUpgrade;
    public int defenseUpgrade;
    public int goldUpgrade;
    public int damageUpgrade;
    public int rofUpgrade;
}

/// <summary>
/// Mejoras que tenía el jugador en UNA estación (reemplaza a Dictionary<string, struct>).
/// </summary>
[System.Serializable]
public class StationUpgradesEntry
{
    public string stationName = "";
    public LocomotiveUpgrades upgrades;
}

/// <summary>
/// Estaciones y mejoras de UNA run (reemplaza a una entrada de Dictionary<int, Dictionary<string, struct>>).
/// </summary>
[System.Serializable]
public class RunUpgradesEntry
{
    public int runPlay = 0;
    public List<StationUpgradesEntry> stations = new List<StationUpgradesEntry>();
}

/// <summary>
/// Un par vagón-cantidad (reemplaza a Dictionary<string, int>).
/// </summary>
[System.Serializable]
public class WagonTypeCountEntry
{
    public string wagonName = "";
    public int count = 0;
}

/// <summary>
/// Vagones con los que el jugador llegó y se fue de UNA estación
/// (reemplaza a una entrada de Dictionary<string, Dictionary<string, int>>).
/// </summary>
[System.Serializable]
public class StationWagonsEntry
{
    public string stationName = "";
    public List<WagonTypeCountEntry> arrivalWagons = new List<WagonTypeCountEntry>();
    public List<WagonTypeCountEntry> departureWagons = new List<WagonTypeCountEntry>();
}

/// <summary>
/// Estaciones de UNA run (reemplaza a una entrada de Dictionary<int, ...>).
/// </summary>
[System.Serializable]
public class RunStationWagonsEntry
{
    public int runPlay = 0;
    public List<StationWagonsEntry> stations = new List<StationWagonsEntry>();
}

/// <summary>
/// Arma comprada en UNA estación (reemplaza a Dictionary<string, string>).
/// </summary>
[System.Serializable]
public class StationWeaponEntry
{
    public string stationName = "";
    public string weaponName = "";
}

/// <summary>
/// Armas compradas en UNA run (reemplaza a una entrada de Dictionary<int, Dictionary<string, string>>).
/// </summary>
[System.Serializable]
public class RunWeaponsEntry
{
    public int runPlay = 0;
    public List<StationWeaponEntry> stations = new List<StationWeaponEntry>();
}

/// <summary>
/// Oro del jugador en UNA estación (reemplaza a Dictionary<string, int>, una vez para entrar y otra para salir).
/// </summary>
[System.Serializable]
public class StationGoldEntry
{
    public string stationName = "";

    public bool arrivalRegistered = false;
    public int goldOnArrival = 0;

    public bool departureRegistered = false;
    public int goldOnDeparture = 0;
}

/// <summary>
/// Oro en las estaciones de UNA run (reemplaza a una entrada de Dictionary<int, Dictionary<string, int>>).
/// </summary>
[System.Serializable]
public class RunGoldEntry
{
    public int runPlay = 0;
    public List<StationGoldEntry> stations = new List<StationGoldEntry>();
}

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

    // Listas en vez de Dictionary, porque JsonUtility no soporta Dictionary.
    public List<WagonCountEntry> wagonInLocomotiveHit = new List<WagonCountEntry>();
    public List<WagonCountEntry> wagonInLocomotiveDestroy = new List<WagonCountEntry>();
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

    // Un título y una línea de texto por cada elemento del diccionario.
    public string wagonHitTitle = "Golpes por vagón:";
    public List<string> wagonHitText = new List<string>();

    public string wagonDestroyTitle = "Destrucciones por vagón:";
    public List<string> wagonDestroyText = new List<string>();
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

    public List<RunResultsEntry> runResults = new List<RunResultsEntry>();

    public List<StationDefeatEntry> stationDefeats = new List<StationDefeatEntry>();

    public List<EnemyKillEntry> enemyKillsTotal = new List<EnemyKillEntry>();

    public List<RunEnemyKillsEntry> enemyKillsPerRun = new List<RunEnemyKillsEntry>();

    // Durante todo el juego
    public WagonManagerInteraction wagonManagerTotal;

    // Por run
    public List<RunWagonManagerEntry> wagonManagerPerRun = new List<RunWagonManagerEntry>();

    public List<RunUpgradesEntry> locomotiveUpgradesPerRun = new List<RunUpgradesEntry>();

    public List<RunStationWagonsEntry> wagonsBetweenStations = new List<RunStationWagonsEntry>();

    public List<RunWeaponsEntry> weaponsPerRun = new List<RunWeaponsEntry>();

    public List<RunGoldEntry> goldPerRun = new List<RunGoldEntry>();
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

    public string runResultsTitle = "Resultados de partidas del jugador";
    public List<string> runResultsText = new List<string>();

    public string stationDefeatsTitle = "Cantidad de derrotas en estaciones";
    public List<string> stationDefeatsText = new List<string>();

    public string enemyKillsTitle = "Muertes de enemigos";

    public string enemyKillsTotalTitle = "Enemigos muertos durante el juego";
    public string enemyKillsRunsPlayedText = "Total de partidas jugadas: 0";
    public List<string> enemyKillsTotalText = new List<string>();

    public string enemyKillsPerRunTitle = "Enemigos muertos por Run";
    public List<string> enemyKillsPerRunText = new List<string>();

    public string wagonManagerTitle = "Interaccion con Manage Wagon";

    public string wagonManagerTotalTitle = "Durante todo el juego";
    public List<string> wagonManagerTotalText = new List<string>();

    public string wagonManagerPerRunTitle = "Durante la Partida";
    public List<string> wagonManagerPerRunText = new List<string>();

    public string locomotiveUpgradesTitle = "Mejoras de locomotora en partida";
    public List<string> locomotiveUpgradesText = new List<string>();

    public string wagonsBetweenStationsTitle = "Vagones entre estaciones";
    public List<string> wagonsBetweenStationsText = new List<string>();

    public string weaponsTitle = "Armas que compro en partidas";
    public List<string> weaponsText = new List<string>();

    public string goldTitle = "Oro en partida";
    public List<string> goldText = new List<string>();

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

