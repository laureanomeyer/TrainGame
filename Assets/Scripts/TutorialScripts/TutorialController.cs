using UnityEngine;
using static TrainStats;
using System.Collections;
using System.Collections.Generic;

public class TutorialController : MonoBehaviour
{
    [SerializeField] GameObject CoalUi;
    [SerializeField] GameObject RunUi;
    [SerializeField] GameObject attackCursor;
    [SerializeField] Transform EnemySpawn;

    private bool started = false;
    private bool firstCash = true;
    private bool firstRepair = false;
    private bool firstkilled = false;
    private List<IWagon> wagons = new();
    private float timer = 5;

    private void Awake()
    {
        TutorialEvents.SetRunStarted(false);
    }

    private void Start()
    {
        CoalUi.SetActive(false);
        RunUi.SetActive(false);

        TutorialEvents.OnSetAttackEnabled += SetAttackUi;
        TutorialEvents.OnStartFuelUse += StartFuelConsumption;
        TutorialEvents.OnStartSpawningEnemies += StartRun;
        TutorialEvents.OnEnemyKilled += CashGoldWagon;

        TutorialEvents.SetCanConsume(false);
        TutorialEvents.SetTimerStarted(false);
        TutorialEvents.SetAttackEnabled(false);
        TutorialEvents.EnableCoalBox(false);
        TutorialEvents.EnableGoldBox(false);
        

        TutorialEvents.SetTutorialTextVisible(true);
        TutorialEvents.SetTutorialText("Presiona WASD para moverte");
    }
    private void Update()
    {
        timer -= 1 * Time.deltaTime;

        if (timer <= 0)
        {
            wagons.Add(RunManager.Instance.ActiveWagons[1]);

            if (GameManager.Instance.CurrentState == GameState.Tutorial) 
            {
                TutorialEvents.SpawnEnemy(EnemySpawn.position, wagons);
            }

            if (!firstRepair)
            {
                firstRepair = true;
                TutorialEvents.SetTutorialTextVisible(true);
                TutorialEvents.SetTutorialText("Esto es un enemigo. Los enemigos atacan el tren. Debes reparar el vagon de oro presionando R");
            }
            timer = float.MaxValue;
        }
    }

    void StartFuelConsumption()
    {
        if (!started)
        {
            CoalUi.SetActive(true);
            started = true;
            TutorialEvents.SetTutorialTextVisible(true);
            TutorialEvents.EnableCoalBox(true);
            TutorialEvents.SetTutorialText("Tu locomotora nunca debe dejar de moverse. Estate atento, que no se agote su combustible");
        }
    }

    void CashGoldWagon()
    {
        if (firstCash)
        {
            firstCash = false;
            TutorialEvents.SetTutorialTextVisible(true);
            TutorialEvents.EnableGoldBox(true);
            TutorialEvents.SetTutorialText("El oro solo esta asegurado en la caja de oro que tiene la locomotora. Recorda recolectarlo y guardarlo tras matar enemigos.");
        }
    }

    void StartRun(bool can)
    {
        RunUi.SetActive(can);
        TutorialEvents.SetCanConsume(true);
        TutorialEvents.SetTutorialTextVisible(true);
        TutorialEvents.SetTutorialText("Bien, este es el trayecto restante hasta la estacion. ¡Tenes que llegar!");
    }

    void SetAttackUi(bool show)
    {
        Cursor.visible = show;

        if (!firstkilled && show)
        {
            firstkilled = true;
            TutorialEvents.SetTutorialTextVisible(true);
            TutorialEvents.SetTutorialText("Excelente, ahora presiona el click izquierdo para disparar una bala y eliminar al enemigo.");
        }
    }

    private void OnDestroy()
    {
        TutorialEvents.OnStartFuelUse -= StartFuelConsumption;
        TutorialEvents.OnStartSpawningEnemies -= StartRun;
        TutorialEvents.OnSetAttackEnabled -= SetAttackUi;
        TutorialEvents.OnEnemyKilled -= CashGoldWagon;
    }
}
