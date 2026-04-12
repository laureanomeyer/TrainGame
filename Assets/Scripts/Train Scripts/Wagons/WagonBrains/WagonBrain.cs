using UnityEngine;

public class WagonBrain : MonoBehaviour, IDamagable, IBuffer
{
    protected float hp;
    [SerializeField] private float currentHp;
    protected float defense;

    protected WagonHP hpController;

    [SerializeField] private Material destroyWagonMaterial;
    [SerializeField] protected Renderer rendererWagon;
    [SerializeField] protected bool canBreak;

    public float CurrentHp => currentHp;
    public float MaxHp => hp;

    public WagonHP HPController => hpController;

    public virtual TrainStats GetStatsBuff(LocomotiveStats baseStats)
    {
        return new TrainStats(0, 0, 0, 0, 0, 0, 0);
    }

    public void Start()
    {
        hp = RunManager.Instance.TrainCopyData.stats.trainMaxHp;
        defense = RunManager.Instance.TrainCopyData.stats.shields;
        hpController = new WagonHP(hp, defense, Break, canBreak);
    }

    public void TakeDamage(float damageAmount)
    {
        hpController.TakeDamage(damageAmount);
        if (hpController.CurrentHp <= 0)
        {
            Break();
        }
    }

    public virtual void Repair(float repairAmount)
    {
        hpController.Repair(Time.deltaTime, repairAmount);
    }

    public void Break()
    {
        rendererWagon.material = destroyWagonMaterial;
    }
}