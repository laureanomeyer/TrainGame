using UnityEngine;

public class HeavyDrumBrain : WagonBrain
{
    private IWeapons playerWeapon;

    [Header("Damage Buffer")]
    [SerializeField] private float damageUpgrade;

    private void Awake()
    {
        WagonType = WagonType.Passive;
        EventBus.Subscribe<OnFinishPlayerInitialize>(SearchForWagons);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        EventBus.Unsubscribe<OnFinishPlayerInitialize>(SearchForWagons);
        Debug.Log("Eventos desuscripto de FastDrum");
    }

    private void SearchForWagons(OnFinishPlayerInitialize trainEvent)
    {
        playerWeapon = GameObject.FindWithTag("Player").GetComponent<PlayerBrain>().PlayerAttackController.Weapon;

        if (playerWeapon.WeaponData.type == WeaponType.Revolver && playerWeapon is IWeaponBuffer weaponBuffer)
        {
            weaponBuffer.BufferDamage(damageUpgrade);
        }
    }

    public override void Break()
    {
        base.Break();

        if (playerWeapon.WeaponData.type == WeaponType.Revolver && playerWeapon is IWeaponBuffer weaponBuffer)
        {
            weaponBuffer.DebuffDamage(damageUpgrade);
        }
    }
}
