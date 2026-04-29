using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WagonBrain : MonoBehaviour, IDamagable
{
    protected float hp;
    protected float defense;

    protected WagonHP hpController;
    protected TrainData dataRef;
    private IWagonID wagonID;
    private DamageFlash Flash;
    private bool broken;

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

    public virtual void Start()
    {
        dataRef = RunManager.Instance.TrainCopyData;
        var statSystem = RunManager.Instance.StatSystem;
        statSystem.OnStatChanged += OnStatChanged;
        Flash = GetComponent<DamageFlash>();
        broken = false;
    }

    public void Update()
    {
        currentHp = HPController.CurrentHp;
    }

    public virtual IEnumerable<StatModifier> GetModifiers()
    {
        yield break;
    }

    public void RegisterModifiers()
    {
        foreach (var mod in GetModifiers())
            RunManager.Instance.StatSystem.AddModifier(mod);
    }

    public void TakeDamage(float damageAmount)
    {
        hpController.TakeDamage(damageAmount);

        if (hpController.CurrentHp > 0) 
            Flash.Flash();

        else  
            Break();       
    }

    public void SetUpWagonHP() 
    {
        var stats = RunManager.Instance.StatSystem;
        float maxHp = SM * stats.GetStat(StatType.MaxHp);
        float def = RES * stats.GetStat(StatType.Defense);
        hpController = new WagonHP(maxHp, def, Break, canBreak);
    }

    public virtual void Repair(float repairAmount)
    {
        hpController.Repair(Time.deltaTime, repairAmount);
    }
    public void SetWagonID (IWagonID wagon)
    {
        this.wagonID = wagon;
    }
    private void OnStatChanged(StatType type, float newValue)
    {
        hpController.OnMaxHpChanged(SM * RunManager.Instance.StatSystem.GetStat(StatType.MaxHp));
    }

    public void Break()
    {
        RunManager.Instance.StatSystem.RemoveModifiersFromSource(this);
        rendererWagon.material = destroyWagonMaterial;

        if (!broken)
            GameEvents.WagonDestroyed();
        broken = true;

        if (wagonID != null)
            GameManager.Instance.TrainData.RemoveWagonID(wagonID);
    }
    public void OnDestroy()
    {
        var statSystem = RunManager.Instance.StatSystem;
        statSystem.OnStatChanged -= OnStatChanged;
    }
}