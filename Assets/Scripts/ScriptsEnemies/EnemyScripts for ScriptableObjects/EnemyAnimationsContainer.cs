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
        StartCoroutine(CowboyDisplacement());
    }

    private IEnumerator CowboyDisplacement()
    {
        Transform cowboy = cowboyAnimator.transform;

        Vector3 startPos = cowboy.position;
        Vector3 targetPos = startPos + new Vector3(cowboyOffset, 0f, 0f);

        float elapsed = 0f;

        while (elapsed < displacementDuration)
        {
            elapsed += Time.deltaTime;

            cowboy.position = Vector3.Lerp(
                startPos,
                targetPos,
                elapsed / displacementDuration
            );

            yield return null;
        }

        cowboy.position = targetPos;
    }

}