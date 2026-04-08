using UnityEngine;

public class WagonBrain : MonoBehaviour, IDamagable, IBuffer
{

    [SerializeField] private LocomotiveStats wagonBuffStats;

    private float hp;
    private float currentHp;
    private float defense;

    protected TrainStats statsBuff;
    private WagonHP hpController;

    [SerializeField] private Material materialDeVagonDestruido;
    [SerializeField] private Renderer rendererWagon;

    public float CurrentHp => currentHp;
    public float MaxHp => hp;

    public TrainStats StatsBuff => statsBuff;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {

        hp = RunManager.Instance.TrainCopyData.stats.trainMaxHp;
        defense = RunManager.Instance.TrainCopyData.stats.shields;
        hpController = new WagonHP(hp, defense, Break);
    }


    // Update is called once per frame
    void Update()
    {
        currentHp = hpController.CurrentHp;
    }


    public void TakeDamage(float damageAmount)
    {
        hpController.TakeDamage(damageAmount);
        if (hpController.CurrentHp <= 0)
        {
            Break();
        }
    }

    public void Repair(float repairAmount)
    {
        hpController.Repair(Time.deltaTime, repairAmount);
    }

    public void Break()
    {
        rendererWagon.material = materialDeVagonDestruido;
    }

    public void AddWagon()
    {
        return;
    }


}
