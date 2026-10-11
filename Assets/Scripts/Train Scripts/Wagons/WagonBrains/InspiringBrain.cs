using UnityEngine;

public class InspiringBrain : WagonBrain
{
    [Header("Rate of Fire Buffer")]
    [SerializeField] private float rofUpgrade;

    private WagonBrain previousWagon;
    private WagonBrain nextWagon;

    private void Awake()
    {
        WagonType = WagonType.Passive;
        EventBus.Subscribe<OnFinishBuildTrain>(SearchForWagons);
        Debug.Log("Eventos suscripto de Inspiring");
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        EventBus.Unsubscribe<OnFinishBuildTrain>(SearchForWagons);
        Debug.Log("Eventos desuscripto de Inspiring");
    }

    private void SearchForWagons(OnFinishBuildTrain trainEvent)
    {
        int index = trainEvent.wagonBrains.IndexOf(this);
        if (index == -1) return;

        if (index > 0)
        {
            if (trainEvent.wagonBrains[index - 1] is MotivationalBrain and IWeaponBuffer weaponBuffer)
            {
                previousWagon = trainEvent.wagonBrains[index - 1];
                weaponBuffer.BufferRoF(rofUpgrade);
                Debug.Log("Vagon anterior bufeado");
            }
        }

        if (index < trainEvent.wagonBrains.Count - 1)
        {
            if (trainEvent.wagonBrains[index + 1] is MotivationalBrain and IWeaponBuffer weaponBuffer)
            {
                nextWagon = trainEvent.wagonBrains[index + 1];
                weaponBuffer.BufferRoF(rofUpgrade);
                Debug.Log("Vagon siguiente bufeado");
            }
        }
    }

    public override void Break()
    {
        base.Break();

        if (previousWagon != null)
        {
            IWeaponBuffer interfaz = (IWeaponBuffer)previousWagon;
            interfaz.DebuffRoF(rofUpgrade);
            Debug.Log("Vagon anterior desbufeado");
        }
        if (nextWagon != null)
        {
            IWeaponBuffer interfaz = (IWeaponBuffer)nextWagon;
            interfaz.DebuffRoF(rofUpgrade);
            Debug.Log("Vagon siguiente desbufeado");
        }
    }
}
