using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
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
    [SerializeField] private float cowboyOffset = 1f;
    [SerializeField] private float displacementDuration = 0.1f;

    public void ActivateCowboyOffset()
    {
        cowboyAnimator.gameObject.transform.position -= new Vector3(1,0,0);
    }

}