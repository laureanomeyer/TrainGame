using UnityEngine;

public class WagonBrain : MonoBehaviour, IDamagable, IBuffer
{
    protected float hp;
    protected float defense;

    protected WagonHP hpController;
    private TrainData dataRef;

    [SerializeField] private float currentHp;
    [SerializeField] private Material destroyWagonMaterial;
    [SerializeField] protected Renderer rendererWagon;
    [SerializeField] protected bool canBreak;
    [SerializeField] private float SM;
    [SerializeField] private float RES;

    public float CurrentHp => currentHp;
    public float MaxHp => hp;

    public WagonHP HPController => hpController;

    public virtual TrainStats GetStatsBuff(LocomotiveStatsSO baseStats)
    {
        return new TrainStats(0, 0, 0, 0, 0, 0, 0);
    }

    public void Start()
    {
        dataRef = RunManager.Instance.TrainCopyData;
        hpController = new WagonHP(
            (SM * (dataRef.LocomotiveStatsMultiplicator.trainMaxHp * dataRef.WagonBuffedStats.trainMaxHp)),
            (RES * (dataRef.LocomotiveStatsMultiplicator.shields * dataRef.WagonBuffedStats.shields)),
            Break,
            canBreak
            );
    }
    public void Update()
    {
        currentHp = HPController.CurrentHp;
    }
    public void TakeDamage(float damageAmount)
    {
        hpController.TakeDamage(damageAmount);
        Debug.Log("took " + damageAmount + " damage");
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