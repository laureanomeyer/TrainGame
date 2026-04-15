using UnityEngine;

public class WagonBrain : MonoBehaviour, IDamagable, IBuffer
{
    protected float hp;
    protected float defense;

    protected WagonHP hpController;
    protected TrainData dataRef;
    private IWagonID wagonID;

    [SerializeField] private float currentHp;
    [SerializeField] private Material destroyWagonMaterial;
    [SerializeField] protected Renderer rendererWagon;
    [SerializeField] protected bool canBreak;
    [SerializeField] protected float SM;
    [SerializeField] protected float RES;

    public float CurrentHp => currentHp;
    public float MaxHp => hp;

    public WagonHP HPController => hpController;
    public IWagonID WagonID => wagonID;

    public virtual TrainStats GetStatsBuff(LocomotiveStatsSO baseStats)
    {
        return new TrainStats(0, 0, 0, 0, 0, 0, 0);
    }

    private void Awake()
    {
        dataRef = RunManager.Instance.TrainCopyData;
        SetUpWagonHP();
    }

    public void Start()
    {
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

    protected void SetUpWagonHP() 
    {
        hpController = new WagonHP(
        (SM * (dataRef.LocomotiveStatsMultiplicator.trainMaxHp * dataRef.WagonBuffedStats.trainMaxHp)),
        (RES * (dataRef.LocomotiveStatsMultiplicator.shields * dataRef.WagonBuffedStats.shields)),
        Break,
        canBreak
        );
    }

    public virtual void Repair(float repairAmount)
    {
        hpController.Repair(Time.deltaTime, repairAmount);
    }
    public void SetWagonID (IWagonID wagon)
    {
        this.wagonID = wagon;
    }

    public void Break()
    {
        rendererWagon.material = destroyWagonMaterial;
        if (wagonID != null ) 
            GameManager.Instance.TrainData.RemoveWagonID(this.wagonID);
        else return;
    }
}