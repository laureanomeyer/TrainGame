using Unity.VisualScripting;
using UnityEngine;
public class EnemyAnimationsContainer : MonoBehaviour
{
    [Header("Cowboy Animator")]
    [SerializeField] private Animator cowboyAnimator;

    [Header("Horse Animator")]
    [SerializeField] private Animator horseAnimator;

    public Animator CowboyAnimator => cowboyAnimator;
    public Animator HorseAnimator => horseAnimator;

    void OnEnable()
    {
        cowboyAnimator.gameObject.transform.position = transform.position;
        horseAnimator.gameObject.transform.position = transform.position;
    }
    public void ActivateCowboyOffset()
    {
        cowboyAnimator.gameObject.transform.position += Vector3.right; 
    }

}