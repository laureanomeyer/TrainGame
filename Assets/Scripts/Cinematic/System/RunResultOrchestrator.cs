using UnityEngine;

public class RunResultOrchestrator : MonoBehaviour
{
    [SerializeField] private CinematicSystem cinematicSystem;

    private RunResult pendingResult;

    private void OnEnable() => EventBus.Subscribe<OnRunEndedEvent>(OnRunEnded);
    private void OnDisable() => EventBus.Unsubscribe<OnRunEndedEvent>(OnRunEnded);

    private void OnRunEnded(OnRunEndedEvent evt)
    {
        pendingResult = evt.Result;

        GameManager.Instance.EnterTransitionState(); //congela el gameplay ni bien termina la run

        cinematicSystem.OnCinematicFinished += HandleCinematicFinished;
        cinematicSystem.CinematicPlay(evt.Result);
    }

    private void HandleCinematicFinished()
    {
        cinematicSystem.OnCinematicFinished -= HandleCinematicFinished;

        if (pendingResult == RunResult.Defeat)
            GameManager.Instance.Defeat();
        else
            RunManager.Instance.OnRunFinished();
    }
}