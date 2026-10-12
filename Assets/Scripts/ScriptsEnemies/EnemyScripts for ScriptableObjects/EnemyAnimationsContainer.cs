using UnityEngine;
public class EnemyAnimationsContainer : MonoBehaviour
{
    [Header("Cowboy Animator")]
    [SerializeField] private Animator cowboyAnimator;

    [Header("Horse Animator")]
    [SerializeField] private Animator horseAnimator;

    public Animator CowboyAnimator => cowboyAnimator;
    public Animator HorseAnimator => horseAnimator;

}