using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CinematicSystem : MonoBehaviour
{
    public event Action OnCinematicFinished;

    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineCamera gameplayCinemachineCamera;
    [SerializeField] private CinemachineCamera cinematicCinemachineCamera;

    [Header("Movement")]
    [SerializeField] private float duration = 3f;

    [Tooltip("La cámara termina en TrainTail X menos este valor.")]
    [SerializeField] private float tailOffsetX = 60f;

    [Header("Priorities")]
    [SerializeField] private int gameplayPriority = 10;
    [SerializeField] private int cinematicPriority = 20;

    private bool isPlaying;

    private void Awake()
    {
        // Al iniciar, la cámara del player gana.
        if (gameplayCinemachineCamera != null)
            gameplayCinemachineCamera.Priority = gameplayPriority;

        if (cinematicCinemachineCamera != null)
            cinematicCinemachineCamera.Priority = 0;
    }

    public void CinematicPlay()
    {
        if (isPlaying)
            return;

        if (gameplayCinemachineCamera == null ||
            cinematicCinemachineCamera == null)
        {
            Debug.LogError("Faltan asignar las Cinemachine Cameras en CinematicSystem.");
            return;
        }

        if (RunManager.Instance == null || RunManager.Instance.TrainTail == null)
        {
            Debug.LogError("No se encontró RunManager o TrainTail.");
            return;
        }

        StartCoroutine(PlayTailCinematic());
    }

    private IEnumerator PlayTailCinematic()
    {
        isPlaying = true;

        Transform tail = RunManager.Instance.TrainTail;

        // La cámara real ya tiene la vista final de Cinemachine gameplay.
        Transform mainCameraTransform = Camera.main.transform;

        // La virtual camera cinematográfica empieza exactamente donde estaba viendo el jugador.
        cinematicCinemachineCamera.transform.position = mainCameraTransform.position;
        cinematicCinemachineCamera.transform.rotation = mainCameraTransform.rotation;

        // Guardamos estos valores UNA sola vez.
        Vector3 startPosition = cinematicCinemachineCamera.transform.position;
        Quaternion startRotation = cinematicCinemachineCamera.transform.rotation;

        // La cinematográfica pasa a ganar prioridad.
        cinematicCinemachineCamera.Priority = cinematicPriority;
        gameplayCinemachineCamera.Priority = gameplayPriority;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / duration);

            // Movimiento suave, pero siempre recto sobre X.
            t = t * t * (3f - 2f * t);

            // Actualiza el destino por si el tren continúa avanzando.
            float targetX = tail.position.x - tailOffsetX;

            // SOLO cambia X.
            cinematicCinemachineCamera.transform.position = new Vector3(
                Mathf.Lerp(startPosition.x, targetX, t),
                startPosition.y,
                startPosition.z
            );

            // No rota durante el recorrido.
            cinematicCinemachineCamera.transform.rotation = startRotation;

            yield return null;
        }

        // Fuerza la posición final exacta.
        cinematicCinemachineCamera.transform.position = new Vector3(
            tail.position.x - tailOffsetX,
            startPosition.y,
            startPosition.z
        );

        OnCinematicFinished?.Invoke();
    }
}