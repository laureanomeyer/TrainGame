using UnityEngine;

public class AnimatorBrain
{
    private Animator animator;
    private AnimationState current;
    private bool locked;
    private AnimationState idleAnimation;
    private int layerIndex;

    public AnimationState Current => current;

    public float CurrentNormalizedTime
    {
        get
        {
            return current.FullPathHash != 0 &&
                TryGetCurrentStateInfo(out AnimatorStateInfo info) &&
                info.fullPathHash == current.FullPathHash
                ? info.normalizedTime
                : -1f;
        }
    }

    public void Init(Animator animator, int layerIndex = 0)
    {
        this.animator = animator;
        this.layerIndex = layerIndex;
        current = default;
        locked = false;

        if (animator == null)
        {
            return;
        }

        if (!IsValidLayer())
        {
            Debug.LogError($"AnimatorBrain cannot use layer {layerIndex} on animator '{animator.name}'.", animator);
            this.animator = null;
            return;
        }

        animator.speed = 1f;
        if (idleAnimation.FullPathHash != 0)
        {
            current = idleAnimation;
            animator.speed = idleAnimation.Speed;
            animator.Play(idleAnimation.FullPathHash, layerIndex, 0f);
        }
    }

    public void SetIdle(AnimationState idle)
    {
        idleAnimation = idle;
    }

    public bool Play(AnimationState next, bool lockAnimation = false, bool overrideLock = false, float fade = 0.1f, float? speed = null)
    {
        if (!IsValidLayer() || next.FullPathHash == 0)
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
        animator.CrossFade(next.FullPathHash, fade, layerIndex, 0f);

        return true;
    }

    public void Unlock()
    {
        locked = false;
    }

    public void ReturnToIdle()
    {
        locked = false;
        if (idleAnimation.FullPathHash != 0)
        {
            Play(idleAnimation, overrideLock: true);
        }
    }

    public bool IsLocked()
    {
        return locked;
    }

    public bool IsPlaying(AnimationState state)
    {
        return state.FullPathHash != 0 &&
            TryGetCurrentStateInfo(out AnimatorStateInfo info) &&
            info.fullPathHash == state.FullPathHash;
    }

    public bool Finished()
    {
        if (current.FullPathHash == 0 || current.Loop)
        {
            return false;
        }

        return TryGetCurrentStateInfo(out AnimatorStateInfo info) &&
            info.fullPathHash == current.FullPathHash &&
            info.normalizedTime >= 1f;
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
        if (!IsValidLayer() || !locked || current.FullPathHash == 0)
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

    private bool IsValidLayer()
    {
        return animator != null && layerIndex >= 0 && layerIndex < animator.layerCount;
    }

    private bool TryGetCurrentStateInfo(out AnimatorStateInfo info)
    {
        if (!IsValidLayer())
        {
            info = default;
            return false;
        }

        info = animator.GetCurrentAnimatorStateInfo(layerIndex);
        return true;
    }
}