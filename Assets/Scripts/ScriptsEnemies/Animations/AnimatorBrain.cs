using UnityEngine;

// TO FIX :
// LOOP FROM ANIMATIONSTAE DOESN�T WORK

public class AnimatorBrain
{
    private Animator animator;

    private AnimationState current;

    private bool locked;

    private AnimationState idleAnimation;

    public AnimationState Current => current;

    public void Init(Animator animator)
    {
        this.animator = animator;

        if (this.animator == null)
        {
            return;
        }

        if (idleAnimation.FullPathHash != 0)
        {
            this.animator.Play(idleAnimation.FullPathHash, 0, 0f);
        }
    }

    public void SetIdle(AnimationState idle)
    {
        idleAnimation = idle;
    }

    public bool Play(AnimationState next, bool lockAnimation = false, bool overrideLock = false, float fade = 0.1f, float? speed = null)
    {
        if (animator == null || next.FullPathHash == 0)
        {
            return false;
        }

        if (locked && !overrideLock)
        {
            return false;
        }

        if (!overrideLock && current.FullPathHash == next.FullPathHash)
        {
            return false;
        }

        current = next;
        locked = lockAnimation;

        animator.speed = speed ?? next.Speed;
        animator.CrossFade(next.FullPathHash, fade, 0, 0f);

        return true;
    }

    public void Unlock()
    {
        locked = false;
    }

    public bool IsLocked()
    {
        return locked;
    }

    public bool IsPlaying(AnimationState state)
    {
        return state.FullPathHash != 0 && animator != null && animator.GetCurrentAnimatorStateInfo(0).fullPathHash == state.FullPathHash;
    }

    public bool Finished()
    {
        if (animator == null || current.FullPathHash == 0 || current.Loop)
        {
            return false;
        }

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        return info.fullPathHash == current.FullPathHash && info.normalizedTime >= 1f;
    }

    public void SetPlaybackSpeed(float speed)
    {
        if (animator != null)
        {
            animator.speed = speed;
        }
    }

    public void ResetPlaybackSpeed()
    {
        if (animator != null)
        {
            animator.speed = 1f;
        }
    }

    public void Tick()
    {
        if (animator == null || !locked || current.FullPathHash == 0)
        {
            return;
        }

        if (current.Loop)
        {
            return;
        }

        if (Finished())
        {
            locked = false;

            if (idleAnimation.FullPathHash != 0 && idleAnimation.FullPathHash != current.FullPathHash)
            {
                Play(idleAnimation, overrideLock: true);
            }
        }
    }
}