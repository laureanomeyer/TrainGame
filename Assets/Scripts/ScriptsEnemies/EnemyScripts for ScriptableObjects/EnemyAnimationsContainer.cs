using UnityEngine;

public class EnemyAnimationsContainer : MonoBehaviour
{
    [Header("Cowboy Animators")]
    [SerializeField] private Animator[] cowboyAnimators;

    [Header("Horse Animators")]
    [SerializeField] private Animator[] horseAnimators;

    private Vector3[] originalCowboyPositions;
    private bool cowboyOffsetApplied;

    public Animator[] CowboyAnimators => cowboyAnimators;
    public Animator[] HorseAnimators => horseAnimators;

    public bool isHumanoid;

    private void Awake()
    {
        originalCowboyPositions = new Vector3[cowboyAnimators.Length];

        for (int i = 0; i < cowboyAnimators.Length; i++)
        {
            if (cowboyAnimators[i] != null)
            {
                originalCowboyPositions[i] =
                    cowboyAnimators[i].transform.localPosition;
            }
        }
    }

    private void OnEnable()
    {
        ResetCowboyPositions();
    }

    private void ResetCowboyPositions()
    {
        cowboyOffsetApplied = false;

        if (originalCowboyPositions == null)
            return;

        for (int i = 0; i < cowboyAnimators.Length; i++)
        {
            if (cowboyAnimators[i] != null)
            {
                cowboyAnimators[i].transform.localPosition =
                    originalCowboyPositions[i];
            }
        }
    }

    public void PlayCowboyAnimation(string stateName, string layerName)
    {
        if (string.IsNullOrEmpty(stateName))
            return;

        foreach (Animator animator in cowboyAnimators)
        {
            if (animator == null)
                continue;

            int layerIndex = animator.GetLayerIndex(layerName);

            if (layerIndex < 0)
                continue;

            animator.SetLayerWeight(layerIndex, 1f);
            animator.Play(stateName, layerIndex, 0f);
        }
    }

    public void PlayHorseAnimation(string stateName)
    {
        if (string.IsNullOrEmpty(stateName))
            return;

        foreach (Animator animator in horseAnimators)
        {
            if (animator != null)
                animator.Play(stateName, 0, 0f);
        }
    }

    public void ActivateCowboyOffset()
    {
        if (!isHumanoid)
            return;

        foreach (Animator animator in cowboyAnimators)
        {
            animator.gameObject.transform.position -= new Vector3(1f, 0f, 0f);
        }
    }
}