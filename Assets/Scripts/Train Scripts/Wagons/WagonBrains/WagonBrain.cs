using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WagonBrain : MonoBehaviour, IDamagable, IWagon
{
    protected float hp;
    protected float defense;

    protected WagonHP hpController;
    protected WagonRenderController renderController;
    private IWagonID wagonID;
    private DamageFlash Flash;
    private Animator animator;
    private bool canBeRepaired = false;
    private WagonMovement wagonMovement;

    private StatSystem stats;
    private TrainData trainData;

    public Transform Transform => transform;
    public bool CanBeRepaired => canBeRepaired;
    public WagonHP HPController => hpController;

    [Header("Wagons data")]
    [SerializeField] private float currentHp;
    [SerializeField] protected bool canBreak;
    [SerializeField] protected float SM;
    [SerializeField] protected float RES;

    [Header("Wagon MovementParams")]
    [SerializeField] protected Transform tail;
    [SerializeField] public GameObject wagonBack;

    [Header("UI")]
    [SerializeField] private Image hpImage;
    [SerializeField] private Image hpBackgroundImage;

    #region Wagon Render/Mesh/Material
    [Header("Wagon renders and materials")]

    [Header("Wagon particles")]
    [SerializeField] public ParticleSystem particles;

    [Header("Wagon destroy material")]
    [SerializeField] public Material destroyWagonMaterial;

    [Header("Wagon destroy mesh")]

    public GameObject WagonNormal;
    public GameObject[] WagonsDestroyed;

    private GameObject wagonDestroyReference;


    [Header("Wagon renderers")]
    [SerializeField] public Renderer wagonTopRender;

    [SerializeField] public MeshFilter wagonTopMeshFilter;

    #endregion

    protected WagonHPWorldUI hpWorldUI;

    public float CurrentHp => currentHp;
    public float MaxHp => hp;
    public WagonMovement WagonMovement => wagonMovement;
    public IWagonID WagonID => wagonID;

    public virtual void Start()
    {
        if(stats != null) stats.OnStatChanged += OnStatChanged;
        animator = GetComponent<Animator>();

        Flash = GetComponent<DamageFlash>();
        trainData = ServiceLocator.Get<TrainData>();

        if (WagonsDestroyed.Count() > 0)
        {
            WagonNormal.SetActive(true);
            int selectedWagon = Random.Range(0, WagonsDestroyed.Length);
            wagonDestroyReference = Instantiate(WagonsDestroyed[selectedWagon], transform);
            wagonDestroyReference.SetActive(false);
        }
    }

    public void FixedUpdate()
    {
        wagonMovement.Move();
    }

    public virtual void StartWagon()
    {
        if (stats == null) { stats = ServiceLocator.Get<StatSystem>(); }

        SetUpWagonHP();
        hpWorldUI = new WagonHPWorldUI(hpImage, hpBackgroundImage);
        hpWorldUI.UpdateHp(hpController.CurrentHp, hpController.MaxHp);
        hpWorldUI.UpdateHp(hpController.CurrentHp, hpController.MaxHp);
        renderController = new WagonRenderController(this);
    }

    public virtual void InitializeWagonMovement(Transform target)
    {
        wagonMovement = new WagonMovement(wagonBack, tail);

        wagonMovement.Initialize(target, transform);
    }

    public virtual IEnumerable<StatModifier> GetModifiers()
    {
        yield break;
    }

    public void RegisterModifiers(StatSystem stats)
    {
        if (stats == null) { stats = ServiceLocator.Get<StatSystem>(); } else this.stats = stats;
            foreach (var mod in GetModifiers())
                stats.AddModifier(mod);
    }

    public virtual void TakeDamage(float damageAmount)
    {
        if(!GameManager.Instance.IsGameplayState) return;

        if (hpController == null) return;
        if (hpController.IsBroken) return;
        if (animator != null) animator.SetTrigger("Damage");

        hpController.TakeDamage(damageAmount);

        currentHp = hpController.CurrentHp;
        canBeRepaired = (hpController.CurrentHp != hpController.MaxHp);

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
        float maxHp = SM * stats.GetStat(StatType.MaxHp);
        float def = RES * stats.GetStat(StatType.Defense);
        
        hpController = new WagonHP(maxHp, def, Break, canBreak);
        
        currentHp = hpController.CurrentHp;
    }

    public virtual void Repair(float repairAmount)
    {
        if (hpController == null) return;
        if (hpController.IsBroken) return;

        hpController.Repair(repairAmount, Time.deltaTime);

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
        hpController.OnMaxHpChanged(SM * stats.GetStat(StatType.MaxHp));
    }

    public void Break()
    {
        if (hpController.IsBroken) return;

        stats.RemoveModifiersFromSource(this);

        renderController.CheckWagonToChangeRender(canBreak);

        EventBus.Publish(new OnWagonDestroyedEvent());

        if ( wagonID != null)
        {
            trainData.RemoveWagonID(wagonID);
        }
    }

    public virtual void SetDestroyed(bool destroyed)
    {
        if (WagonsDestroyed.Count() > 0)
        {
            wagonDestroyReference.SetActive(destroyed);
            WagonNormal.SetActive(!destroyed);
        }
    }

    public virtual void OnDestroy()
    {
        stats.OnStatChanged -= OnStatChanged;
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