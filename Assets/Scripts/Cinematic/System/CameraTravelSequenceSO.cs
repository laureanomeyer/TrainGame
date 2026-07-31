using UnityEngine;

[CreateAssetMenu(menuName = "Cinematic/System/Camera Travel Sequence")]
public class CameraTravelSequenceSO : ScriptableObject
{
    [Header("Travel Speed")]
    public float travelSpeed = 15f;
    public float minTravelDuration = 0.3f;
    public float maxTravelDuration = 1.2f;

    [Header("Timing")]
    public float holdDuration = 1f;

    public AnimationCurve travelCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve returnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Framing")]
    [Tooltip("Offset en espacio del MUNDO, no relativo a la rotación del target.")]
    public Vector3 worldOffsetFromTarget = new Vector3(4f, 6f, -4f);

    public bool lookAtTarget = true;

    [Tooltip("Ignora lookAtTarget y usa una rotación fija en euler. Para tomas con encuadre exacto.")]
    public bool useFixedRotation = false;

    [Tooltip("Rotación destino en euler. Solo se usa si useFixedRotation está activo.")]
    public Vector3 fixedEulerRotation = new Vector3(20f, -90f, 0f);

    [Header("Behaviour")]
    [Tooltip("Ejes sobre los que la cámara puede desplazarse. Los no marcados quedan fijos en el valor de origen.")]
    public TravelAxis travelAxes = TravelAxis.All;

    [Tooltip("Recalcula el destino cada frame. Necesario si el target se mueve (tren en marcha).")]
    public bool trackTarget = false;

    [Tooltip("Al terminar, vuelve a la posición inicial y devuelve la prioridad. Off para cinemáticas de fin de run.")]
    public bool returnToOrigin = true;
}