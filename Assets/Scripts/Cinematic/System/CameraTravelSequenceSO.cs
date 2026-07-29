using UnityEngine;
[CreateAssetMenu(menuName = "Cinematic/System/Camera Travel Sequence")]
public class CameraTravelSequenceSO : ScriptableObject
{
    [Header("TravelSpeed")]
    public float travelSpeed = 15f;
    public float minTravelDuration = 0.3f;
    public float maxTravelDuration = 1.2f;


    public float holdDuration = 1f;
    public float returnDuration = 0.6f;

    public AnimationCurve travelCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve returnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Framing")]
    [Tooltip("Offset en espacio del MUNDO, no relativo a la rotación del wagon.")]
    public Vector3 worldOffsetFromTarget = new Vector3(4f, 6f, -4f);
    public bool lookAtTarget = true;
}
