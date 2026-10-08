using System;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

public class FastDrumBrain : WagonBrain
{
    private IWeapons playerWeapon;

    [Header("Rate of Fire Buffer")]
    [SerializeField] private float rofUpgrade;

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
            weaponBuffer.BufferRoF(rofUpgrade);
        }

        /*
        //Codigo para inspiring
        WagonBrain previousWagon;
        WagonBrain nextWagon;

        int index = trainEvent.wagonBrains.IndexOf(this);
        if (index == -1) return;

        if (index > 0)
        {
            previousWagon = trainEvent.wagonBrains[index - 1];
            Debug.Log(previousWagon.ToString());
        }

        if (index < trainEvent.wagonBrains.Count - 1)
        {
            nextWagon = trainEvent.wagonBrains[index + 1];
            Debug.Log(nextWagon.ToString());
        }
        */
    }

    public override void Break()
    {
        base.Break();

        if (playerWeapon.WeaponData.type == WeaponType.Revolver && playerWeapon is IWeaponBuffer weaponBuffer)
        {
            weaponBuffer.DebuffRoF(rofUpgrade);
        }
    }
}
