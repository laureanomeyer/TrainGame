using UnityEngine;


public class LevelEndTile : MonoBehaviour
{
    public const string EntranceAnchorKey = "LevelEndEntrance";

    [Tooltip("Empty en la boca del túnel, a la altura de la vía y centrado sobre ella.")]
    [SerializeField] private Transform entrance;

    private ICinematicActorRegistry registry;

    public Transform Entrance => entrance;


    public Vector3 LocalEntranceOffset =>
        entrance == null ? Vector3.zero : Quaternion.Inverse(transform.rotation) * (entrance.position - transform.position);

    private void OnEnable()
    {
        if (entrance == null)
        {
            Debug.LogError($"[LevelEndTile] '{name}' no tiene asignada la Entrance.");
            return;
        }

        registry = ServiceLocator.Get<ICinematicActorRegistry>();
        registry?.RegisterDynamic(EntranceAnchorKey, entrance);
    }

    private void OnDisable()
    {
        registry?.UnregisterDynamic(EntranceAnchorKey);
    }
}