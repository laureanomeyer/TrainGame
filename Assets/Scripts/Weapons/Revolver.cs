using System;
using System.Collections;
using System.Net.Sockets;
using UnityEngine;

public class Revolver : MonoBehaviour, IWeapons
{
    public string id => "revolver";

    //Tipos de balas que utiliza el arma
    [Header("BulletType")]
    [SerializeField] private BulletTypeScriptable bulletScriptable;

    [Header("Ammun")]
    [SerializeField] private int bulletAmmunition;

    private int currentAmmunition;
    public float CurrentAmmunition { get => currentAmmunition; }

    [Header("Reloud time")]
    [SerializeField] private float reloadDuration;
    public float ReloadDuration { get => reloadDuration; }

    [Header("Rate of fire")]
    [SerializeField] private float rateOfFire;
    public float RateOfFire { get => rateOfFire; }


    //Referencia a la pool de balas
    private BulletPool bulletPool;

    public void Shoot(Transform spawnPoint)
    {
        bulletPool.ShootObject(spawnPoint.position, spawnPoint.rotation, bulletScriptable);
        currentAmmunition -= 1;

    }

    public void SetPool(BulletPool pool)
    {
        bulletPool = pool;
    }

    public void Relood()
    {
        currentAmmunition = bulletAmmunition;
    }
}
