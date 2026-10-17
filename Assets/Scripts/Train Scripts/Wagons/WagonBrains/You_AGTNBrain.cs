using UnityEngine;

public class You_AGTNBrain : WagonBrain
{
    private IWeapons playerWeapon;

    [Header("Rate of Fire Buffer")]
    [SerializeField] private float rofUpgrade;

    [Header("Damage Buffer")]
    [SerializeField] private float damageUpgrade;

    private void Awake()
    {
        WagonType = WagonType.Passive;
        EventBus.Subscribe<OnFinishPlayerInitialize>(SearchForPlayer);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        EventBus.Unsubscribe<OnFinishPlayerInitialize>(SearchForPlayer);
        Debug.Log("Eventos desuscripto de FastDrum");
    }

    private void SearchForPlayer(OnFinishPlayerInitialize trainEvent)
    {
        playerWeapon = GameObject.FindWithTag("Player").GetComponent<PlayerBrain>().PlayerAttackController.Weapon;

        if (playerWeapon is SpecerRifle_Weapon && playerWeapon is IWeaponBuffer weaponBuffer)
        {
            weaponBuffer.BufferRoF(rofUpgrade);
            weaponBuffer.BufferDamage(damageUpgrade);
            Debug.Log("Spencer buffed");
        }
    }

    public override void Break()
    {
        base.Break();

        if (playerWeapon is SpecerRifle_Weapon && playerWeapon is IWeaponBuffer weaponBuffer)
        {
            weaponBuffer.DebuffRoF(rofUpgrade);
            weaponBuffer.DebuffDamage(damageUpgrade);
            Debug.Log("Spencer debuffed");
        }
    }
}
