using UnityEngine;

public class WagonBrain : MonoBehaviour, ITrainBrain
{

    [SerializeField] private float hp;
    [SerializeField] private float currentHp;
    [SerializeField] private float defense;
    [SerializeField] private string wagonType;
    private WagonHP hpController;

    [SerializeField] private Material materialDeVagonDestruido;
    [SerializeField] private Renderer rendererWagon;

    public float CurrentHp => currentHp;
    public float MaxHp => hp;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hpController = new WagonHP(hp, defense, BreakDown);
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
            BreakDown();
        }
    }

    public void Repair(float repairAmount)
    {
        hpController.Repair(Time.deltaTime, repairAmount);
    }

    public void BreakDown()
    {
        rendererWagon.material = materialDeVagonDestruido;
    }
}
