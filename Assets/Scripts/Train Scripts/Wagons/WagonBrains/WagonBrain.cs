using System.Collections.Generic;
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
    private WagonMovement wagonMovement;

    private StatSystem stats;
    private TrainData trainData;
    private bool statsSubscribed;
    private bool isBroken;

    public Transform Head => transform;
    public Transform Tail => tail;

    // Calculado en vez de cacheado: antes solo se actualizaba en TakeDamage, así que
    // quedaba en true con el vagón lleno o roto y el player se "trababa" reparando.
    public bool CanBeRepaired =>
        hpController != null &&
        !hpController.IsBroken &&
        hpController.CurrentHp < hpController.MaxHp;

    public bool IsBroken => isBroken;
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
    [SerializeField] private CanvasGroup repairIcon;
    [SerializeField] private CanvasGroup hpCanvas;

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

    public Camera Cam => Camera.main;

    [SerializeField] private WagonType wagonType;
    public WagonType WagonType { protected set => wagonType = value; get => wagonType; }


    bool isOnScreen;

    public virtual void Start()
    {
        SubscribeToStats();
        animator = GetComponent<Animator>();

        Flash = GetComponent<DamageFlash>();
        trainData = ServiceLocator.Get<TrainData>();

        if (WagonsDestroyed != null && WagonsDestroyed.Length > 0)
        {
            WagonNormal.SetActive(true);
            int selectedWagon = Random.Range(0, WagonsDestroyed.Length);
            wagonDestroyReference = Instantiate(WagonsDestroyed[selectedWagon], transform);
            wagonDestroyReference.SetActive(false);
        }
    }

    public void FixedUpdate()
    {
        // Puede correr antes de InitializeWagonMovement
        if (wagonMovement != null)
            wagonMovement.Move();
    }

    public virtual void StartWagon()
    {
        if (stats == null) { stats = ServiceLocator.Get<StatSystem>(); }
        SubscribeToStats(); // si Start corrió antes que StartWagon, stats era null y nunca se suscribía

        SetUpWagonHP();
        hpWorldUI = new WagonHPWorldUI(hpImage, repairIcon, hpCanvas);
        hpWorldUI.UpdateHp(hpController.CurrentHp, hpController.MaxHp);
        renderController = new WagonRenderController(this);
    }

    private void SubscribeToStats()
    {
        if (statsSubscribed || stats == null) return;
        stats.OnStatChanged += OnStatChanged;
        statsSubscribed = true;
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
        // Antes: si venía null se asignaba al PARÁMETRO, no al campo, y this.stats quedaba null
        this.stats = stats != null ? stats : ServiceLocator.Get<StatSystem>();
        SubscribeToStats();

        foreach (var mod in GetModifiers())
            this.stats.AddModifier(mod);
    }

    public virtual void TakeDamage(float damageAmount)
    {
        if (!GameManager.Instance.IsGameplayState) return;

        if (hpController == null) return;
        if (isBroken || hpController.IsBroken) return;
        if (animator != null) animator.SetTrigger("Damage");

        // Puede disparar Break() vía el callback de WagonHP
        hpController.TakeDamage(damageAmount);
        isOnScreen = CameraView.IsInsideCamera(transform.position, Cam);
        AudioManager.Instance.PlayOnScreen($"SFXWagonHit{UnityEngine.Random.Range(1, 4)}", isOnScreen);

        currentHp = hpController.CurrentHp;

        if (hpWorldUI != null)
        {
            hpWorldUI.UpdateHp(hpController.CurrentHp, hpController.MaxHp);
        }

        if (hpController.CurrentHp > 0)
        {
            if (Flash != null)
            {
                Flash.Flash();
            }
        }
        else
        {
            Break(); // protegido por isBroken, no se ejecuta dos veces
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

    public void SetWagonID(IWagonID wagon)
    {
        this.wagonID = wagon;
    }

    private void OnStatChanged(StatType type, float newValue)
    {
        if (hpController == null) return;

        hpController.OnMaxHpChanged(SM * stats.GetStat(StatType.MaxHp));
        currentHp = hpController.CurrentHp;

        if (hpWorldUI != null)
            hpWorldUI.UpdateHp(hpController.CurrentHp, hpController.MaxHp);
    }

    public virtual void Break()
    {
        // Guard propio: WagonHP ya marca IsBroken antes de llamar a este callback,
        // así que no se puede usar hpController.IsBroken acá.
        if (isBroken) return;
        isBroken = true;

        if (stats != null) stats.RemoveModifiersFromSource(this);

        if (renderController != null) renderController.CheckWagonToChangeRender(canBreak);

        EventBus.Publish(new OnWagonDestroyedEvent(this));

        RunManager.Instance.OnWagonDestroyed(wagonID, this);
    }

    public virtual void SetDestroyed(bool destroyed)
    {
        if (wagonDestroyReference != null)
        {
            wagonDestroyReference.SetActive(destroyed);
            WagonNormal.SetActive(!destroyed);
        }
    }

    public virtual void OnDestroy()
    {
        if (statsSubscribed && stats != null)
            stats.OnStatChanged -= OnStatChanged;
    }

    public void ShowHpBar()
    {
        if (hpWorldUI != null)
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

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (renderController != null) renderController.DeactivateWagonTop();

            if (GameManager.Instance.IsTutorial)
            {
                EventBus.Publish(new OnShowRepairIconEvent(true));
            }
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (renderController != null) renderController.ActivateWagonTop();

            if (GameManager.Instance.IsTutorial)
            {
                EventBus.Publish(new OnShowRepairIconEvent(false));
            }
        }
    }
}