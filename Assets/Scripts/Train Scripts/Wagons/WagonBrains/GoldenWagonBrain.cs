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
    [SerializeField] private Mesh baseFloorWagonMesh;
    [SerializeField] private Mesh baseBodyWagonMesh;
    [SerializeField] private GameObject[] goldCoins;

    private void Awake()
    {
        GameManager.Instance.Session.TrainData.SetGoldBox(goldBox);
    }
    public override void Start()
    {
        base.Start();
        collector = new GoldCollector(hpController, currentGoldUI, storageCapacity, setGoldCoins);
    }

    public override void Repair(float repairAmount)
    {
        if (hpController.IsBroken == true & hpController.CurrentHp > 0)
        {
            renderController.SetWagonMeshAndMaterial(baseFloorWagonMesh, baseBodyWagonMesh);
            hpController.IsBroken = false;
        }

        hpController.Repair(repairAmount, Time.deltaTime);
        if (GameManager.Instance.CurrentState == GameState.Tutorial)
            TutorialEvents.SetAttackEnabled(true);

        if (hpWorldUI != null)
        {
            hpWorldUI.UpdateHp(hpController.CurrentHp, hpController.MaxHp);
        }
    }
    public override void TakeDamage(float damageAmount)
    {
        base.TakeDamage(damageAmount);
        if (hpController.CurrentHp <= 0 && hpController != null)
        {
            collector.EmptyGold();
        }

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

    public override void OnDestroy()
    {
        base.OnDestroy();
        collector.ActivateOnDestroy();
    }
}
