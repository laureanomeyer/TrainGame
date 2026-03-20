using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] EnemyData data;

    public Transform Target { get; private set; }


    //public IEnemyMovement Movement => data.movement;
    public IEnemyAttack Attack => data.attack;
    public IEnemyBrain Brain => data.brain;


    void Awake()
    {
        Attack.Attack(this);
        Brain.Begin(this);
    }

}