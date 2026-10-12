using UnityEngine;

public sealed class EnemyVisualAnimatorBindings : MonoBehaviour
{
    [SerializeField] private Animator cowboyAnimator;
    [SerializeField] private Animator mountAnimator;

    public Animator CowboyAnimator => cowboyAnimator;
    public Animator MountAnimator => mountAnimator;
}
