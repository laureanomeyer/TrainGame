using System.Collections.Generic;

// =========================================================
//  CONTENEDORES (reemplazan a los Dictionary, que JsonUtility no serializa)
// =========================================================

/// <summary>
/// Un par índice-cantidad (reemplaza a una entrada de Dictionary).
/// Se usará en la parte global.
/// </summary>
[System.Serializable]
public class WagonCountEntry
{
    public int wagonIndex = 0;
    public int count = 0;
}

/// <summary>
/// Resultado de UNA partida: número de run, si ganó o perdió, y la última estación en la que estuvo.
/// </summary>
[System.Serializable]
public class RunResultsEntry
{
    public int runPlay = 0;
    public bool result;

    // Las partidas guardadas antes de este cambio no tienen este campo: quedan en "unknown".
    public string lastStationName = "unknown";
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
/// Oro del jugador en UNA estación (una vez para entrar y otra para salir).
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
//  DATOS GLOBALES (pendiente: se retomará más adelante)
// =========================================================

/// <summary>
/// Datos globales. Por ahora no se envían a Firebase.
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

// =========================================================
//  DATOS DE USUARIO
// =========================================================

/// <summary>
/// Información de UN usuario. Es el JSON que se sube a Firebase en users/{userId}.
/// Todas las variables empiezan en 0 al crear un usuario nuevo.
/// </summary>
[System.Serializable]
public class UserData
{
    // ID único numérico asignado por Firebase (0 = todavía pendiente de asignar)
    public int userId = 0;

    // Uid anónimo del equipo que creó al usuario (lo usan las reglas de seguridad de Firebase)
    public string ownerUid = "";

    // Fecha de creación en UTC (formato ISO 8601)
    public string createdAt = "";

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