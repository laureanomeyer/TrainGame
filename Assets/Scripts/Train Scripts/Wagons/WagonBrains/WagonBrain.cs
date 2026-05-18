using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WagonBrain : MonoBehaviour, IDamagable
{
    protected float hp;
    protected float defense;

    protected WagonHP hpController;
    private WagonRenderController renderController;
    private IWagonID wagonID;
    private DamageFlash Flash;
    private Animator animator;

    private bool broken;
    public bool Broken => broken;

    [Header("Wagons data")]
    [SerializeField] private float currentHp;
    [SerializeField] protected bool canBreak;
    [SerializeField] protected float SM;
    [SerializeField] protected float RES;

    [Header("UI")]
    [SerializeField] private Image hpImage;
    [SerializeField] private Image hpBackgroundImage;

    #region Wagon Render/Mesh/Material
    [Header("Wagon renders and materials")]

    [Header("Wagon destroy material")]
    [SerializeField] public Material destroyWagonMaterial;

    [Header("Wagon destroy mesh")]
    [SerializeField] public Mesh floorMeshDestroyWagon;
    [SerializeField] public Mesh bodyMeshDestroyWagon;

    [Header("Wagon renderers")]
    [SerializeField] public Renderer floorRenderWagon;
    [SerializeField] public Renderer bodyRenderWagon;
    [SerializeField] public Renderer topRenderWagon;

    [Header("Wagon mesh filter")]
    [SerializeField] public MeshFilter floorMeshFilterWagon;
    [SerializeField] public MeshFilter bodyMeshFilterWagon;
    [SerializeField] public MeshFilter topMeshFilterWagon;
    #endregion

    protected WagonHPWorldUI hpWorldUI;

    public float CurrentHp => currentHp;
    public float MaxHp => hp;

    public WagonHP HPController => hpController;
    public IWagonID WagonID => wagonID;

    public virtual void Start()
    {
        var statSystem = RunManager.Instance.StatSystem;
        statSystem.OnStatChanged += OnStatChanged;
        animator = GetComponent<Animator>();

        Flash = GetComponent<DamageFlash>();
        broken = false;
    }

    public virtual void StartWagon()
    {
        SetUpWagonHP();
        hpWorldUI = new WagonHPWorldUI(hpImage, hpBackgroundImage);
        hpWorldUI.UpdateHp(hpController.CurrentHp, hpController.MaxHp);
        hpWorldUI.UpdateHp(hpController.CurrentHp, hpController.MaxHp);
        renderController = new WagonRenderController(this);
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

    public virtual void TakeDamage(float damageAmount)
    {
        if(hpController == null) return;
        if (broken) return;
        if (animator != null) animator.SetTrigger("Damage");

        hpController.TakeDamage(damageAmount);

        currentHp = hpController.CurrentHp;

        if ( hpWorldUI != null)
        {
            hpWorldUI.UpdateHp(hpController.CurrentHp, hpController.MaxHp);
        }   

        if ( hpController.CurrentHp > 0)
        {
            if ( Flash != null)
            {
                Flash.Flash();
            }
        }
        else
        {
            Break();
        }
    }

    public void SetUpWagonHP() 
    {
        var stats = RunManager.Instance.StatSystem;

        float maxHp = SM * stats.GetStat(StatType.MaxHp);
        float def = RES * stats.GetStat(StatType.Defense);
        
        hpController = new WagonHP(maxHp, def, Break, canBreak);
        
        currentHp = hpController.CurrentHp;
    }

    public virtual void Repair(float repairAmount)
    {
        if (hpController == null) return;
        if (broken) return;

        hpController.Repair(repairAmount * Time.deltaTime);

        currentHp = hpController.CurrentHp;

        if (hpWorldUI != null)
        {
            hpWorldUI.UpdateHp(hpController.CurrentHp, hpController.MaxHp);
        }
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
        if (broken) return;

        RunManager.Instance.StatSystem.RemoveModifiersFromSource(this);

        renderController.CheckWagonToChangeRender(canBreak);

        GameEvents.WagonDestroyed();

        broken = true;

        if ( wagonID != null)
        {
            GameManager.Instance.Session.TrainData.RemoveWagonID(wagonID);
        }
    }
    public virtual void OnDestroy()
    {
        var statSystem = RunManager.Instance.StatSystem;
        statSystem.OnStatChanged -= OnStatChanged;
    }

    public void ShowHpBar()
    {
        if(hpWorldUI != null)
        {
            hpWorldUI.SetVisible(true);
        }
    }

    public void HideHpBar()
    {
        if (hpWorldUI != null)
        {
            hpWorldUI.SetVisible(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            renderController.DeactivateWagonTop();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            renderController.ActivateWagonTop();
        }
    }
}