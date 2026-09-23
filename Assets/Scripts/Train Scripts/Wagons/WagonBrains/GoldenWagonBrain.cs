using System;
using TMPro;
using UnityEngine;

public class GoldenWagonBrain : WagonBrain
{
    private GoldCollector collector;
    public GoldCollector Collector => collector;

    [Header("Gold wagon data")]
    [SerializeField] private float storageCapacity;
    [SerializeField] private TextMeshProUGUI currentGoldUI;
    [SerializeField] private Transform goldBox;

    [Header("Models")]
    [SerializeField] private GameObject[] goldCoins;
    [SerializeField] private Transform backDoor;

    private float closedRotation;
    private float openRotation;
    private float fixedY;
    private float fixedZ;
    private bool tutorialTargetAssigned;
    private bool firstRepair = false;

    private void Awake()
    {
        var dataRef = ServiceLocator.Get<TrainData>();
        dataRef.SetGoldBox(goldBox);
        WagonType = WagonType.Gold;
        closedRotation = backDoor.localEulerAngles.x;
        openRotation = closedRotation - 110f;
        fixedY = backDoor.localEulerAngles.y;
        fixedZ = backDoor.localEulerAngles.z;
    }
    public override void Start()
    {
        base.Start();
        collector = new GoldCollector(hpController, currentGoldUI, storageCapacity, setGoldCoins);
        ServiceLocator.Register<WagonHP>(hpController);
        EventBus.Subscribe<OnActivateGoldWagon>(ActivateGoldWagon);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        collector.ActivateOnDestroy();
    }

    public override void Repair(float repairAmount)
    {
        HandleBackDoor();
        ChangeBrokenState();

        hpController.Repair(repairAmount, Time.deltaTime);
        if (GameManager.Instance.CurrentState == GameState.Tutorial && !firstRepair && hpController.CurrentHp == hpController.MaxHp)
        {
            EventBus.Publish(new OnAdvanceTutorialStep());    
            firstRepair = !firstRepair;
        }

        if (hpWorldUI != null)
        {
            hpWorldUI.UpdateHp(hpController.CurrentHp, hpController.MaxHp);
        }
    }
    public override void TakeDamage(float damageAmount)
    {
        base.TakeDamage(damageAmount);
        HandleBackDoor();
        if (hpController.CurrentHp <= 0 && hpController != null)
        {
            collector.EmptyGold();
        }
    }

    private void ChangeBrokenState()
    {
        if (hpController.IsBroken == true && hpController.CurrentHp > 0)
        {
            hpController.IsBroken = false;
            EventBus.Publish(new OnGoldenWagonIsRepairedEvent(this));
        }
    }

    private void HandleBackDoor()
    {
        float t = 1f - (hpController.CurrentHp / hpController.MaxHp);
        float targetX = Mathf.Lerp(closedRotation, openRotation, t);

        Quaternion targetRotation = Quaternion.Euler(targetX, fixedY, fixedZ);
        backDoor.localRotation = Quaternion.RotateTowards(backDoor.localRotation, targetRotation, 30f);
    }
    public void setGoldCoins(float currentGold, float maxGold)
    {
        if(currentGold <= 0)
        {
            DeactivateAllCoins();
        }
        else
        {
            if (currentGold >= maxGold / 1.5f && goldCoins[3].activeInHierarchy == false)
            {
                goldCoins[3].SetActive(true);
            }
            else if (currentGold > maxGold / 2f && goldCoins[2].activeInHierarchy == false)
            {
                goldCoins[2].SetActive(true);
            }
            else if (currentGold > maxGold / 3f && goldCoins[1].activeInHierarchy == false)
            {
                goldCoins[1].SetActive(true);
            }
            else if (currentGold > maxGold / 4f && goldCoins[0].activeInHierarchy == false)
            {
                goldCoins[0].SetActive(true);
            }
        }
    }

    private void DeactivateAllCoins()
    {
        foreach (var coin in goldCoins)
        {
            coin.SetActive(false);
        }
    }

    private void ActivateGoldWagon(OnActivateGoldWagon ev)
    {
        HPController.forceHp(25f);
        HPController.forceHp(1f);
    }

}
