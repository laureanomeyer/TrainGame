using UnityEngine;

public class WagonBrain : MonoBehaviour
{

    [SerializeField] private float hp;
    [SerializeField] private float currentHp;
    [SerializeField] private float defense;
    [SerializeField] private string wagonType;
    private WagonHP hpController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hpController = new WagonHP(hp, defense);
    }


    // Update is called once per frame
    void Update()
    {
        currentHp = hpController.CurrentHp;
    }


    public void TakeDamage(float damageAmount)
    {
        hpController.TakeDamage(damageAmount);
        Debug.Log(hpController.CurrentHp);
    }

    public void Repair(float repairAmount)
    {
        hpController.Repair(Time.deltaTime, repairAmount);
        Debug.Log(hpController.CurrentHp);
    }
}
