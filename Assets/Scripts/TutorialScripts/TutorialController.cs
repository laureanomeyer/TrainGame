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
    private List<IWagon> wagons = new();
    float timer = 5;

    private void Awake()
    {
        TutorialEvents.SetRunStarted(false);
    }
    private void Start()
    {
        CoalUi.SetActive(false);
        RunUi.SetActive(false);
        TutorialEvents.SetCanConsume(false);
        TutorialEvents.SetTimerStarted(false);
        TutorialEvents.OnSetAttackEnabled += SetAttackUi;
        TutorialEvents.SetAttackEnabled(false);
        TutorialEvents.OnStartFuelUse += StartFuelConsumption;
        TutorialEvents.OnStartSpawningEnemies += StartRun;

        TutorialEvents.SetTutorialTextVisible(true);
        TutorialEvents.SetTutorialText("Presiona WASD para moverte");
    }
    private void Update()
    {
        timer -= 1 * Time.deltaTime;

        if (timer <= 0)
        {
            wagons.Add(RunManager.Instance.ActiveWagons[1]);
            TutorialEvents.SpawnEnemy(EnemySpawn.position, wagons);

            TutorialEvents.SetTutorialTextVisible(true);
            TutorialEvents.SetTutorialText("Esto es un enemigo. Los enemigos dañan el tren. Debes reparar el vagon de oro presionando R");

            timer = 10000000000000;
        }
    }

    void StartFuelConsumption()
    {
        TutorialEvents.SetCanConsume(true);
        CoalUi.SetActive(true);

        TutorialEvents.SetTutorialTextVisible(true);
        TutorialEvents.SetTutorialText("Tu locomotora nunca debe dejar de moverse. Estate atento, que no se agote su combustible");
    }

    void StartRun(bool can)
    {
        RunUi.SetActive(can);

        TutorialEvents.SetTutorialTextVisible(true);
        TutorialEvents.SetTutorialText("Bien, este es el trayecto restante hasta la estación. ¡Tenés que llegar!");
    }

    void SetAttackUi(bool show)
    {
        Cursor.visible = show;
        attackCursor.SetActive(show);

        TutorialEvents.SetTutorialTextVisible(true);
        TutorialEvents.SetTutorialText("Excelente, ahora presiona el click izq. para disparar una bala y eliminar al enemigo.");
    }

    private void OnDestroy()
    {
        TutorialEvents.OnStartFuelUse -= StartFuelConsumption;
        TutorialEvents.OnStartSpawningEnemies -= StartRun;
        TutorialEvents.OnSetAttackEnabled -= SetAttackUi;
    }
}
