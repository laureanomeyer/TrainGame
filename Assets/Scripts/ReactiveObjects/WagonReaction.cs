using UnityEngine;

public class WagonReaction : MonoBehaviour, IReactiveObject
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void OnDamage()
    {
        animator.SetTrigger("Damage");
    }
}
