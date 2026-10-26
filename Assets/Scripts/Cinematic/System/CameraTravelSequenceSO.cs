using UnityEngine;
using UnityEngine.Serialization;

public enum OffsetSpace
{
    World,
    Target
}

[CreateAssetMenu(menuName = "Cinematic/System/Camera Travel Sequence")]
public class CameraTravelSequenceSO : ScriptableObject
{
    [Header("Travel Speed (ida)")]
    public float travelSpeed = 15f;
    public float minTravelDuration = 0.3f;
    public float maxTravelDuration = 1.2f;

    [Header("Return Speed (vuelta)")]
    public float returnSpeed = 15f;
    public float minReturnDuration = 0.3f;
    public float maxReturnDuration = 1.2f;

    [Header("Timing")]
    public float holdDuration = 1f;

    public AnimationCurve travelCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve returnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Zoom (FOV)")]
    public bool useFOVZoom = false;
    public float startFov = 40f;
    public float endFov = 18f;

    [Header("Framing")]
    [Tooltip("World: offsets en ejes del mundo. Target: offsets relativos a la rotación del target (X = derecha, Z = forward del target).")]
    public OffsetSpace offsetSpace = OffsetSpace.World;

    [Tooltip("Posición de la cámara relativa al target.")]
    [FormerlySerializedAs("worldOffsetFromTarget")]
    public Vector3 offsetFromTarget = new Vector3(4f, 6f, -4f);

    [Tooltip("Punto al que mira la cámara, relativo al target. Útil si el pivot del vagón no está en el centro.")]
    public Vector3 lookAtOffset = Vector3.zero;

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

    [Tooltip("Usa el centro visual del target (bounds de sus renderers) como ancla en vez del pivot. " +
         "Con lookAtTarget, deja el target centrado en pantalla. Los offsets se miden desde ese centro.")]
    public bool anchorToBoundsCenter = false;
}