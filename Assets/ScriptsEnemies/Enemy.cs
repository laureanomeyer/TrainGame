using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] EnemyData data;
    [SerializeField] GameObject target;

    public GameObject Target => target;

    public IEnemyWeapon Weapon => data.weapon;
    public IEnemyMovement Movement => data.movement;
    public IEnemyAttack Attack => data.attack;
    public IEnemyBrain Brain => data.brain;
    public float Speed => data.speed;




    void Awake()
    {
        Brain.Begin(this);
    }

    private void Update()
    {
        Attack.Attack(this);
        if (Movement != null) 
        {
            Movement.Move(this);
        }
    }

}