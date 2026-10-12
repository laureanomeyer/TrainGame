[System.Serializable]
public struct AnimationState
{
    public string StatePath;
    public bool Loop;
    public float Speed;

    public int FullPathHash => string.IsNullOrWhiteSpace(StatePath) ? 0 : UnityEngine.Animator.StringToHash(StatePath);

    public AnimationState(string statePath, bool loop = false, float speed = 1f)
    {
        StatePath = statePath;
        Loop = loop;
        Speed = speed;
    }
}