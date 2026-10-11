public readonly struct AnimationState
{
    public readonly int FullPathHash;
    public readonly bool Loop;
    public readonly float Speed;

    public AnimationState(int fullPathHash, bool loop = false, float speed = 1f)
    {
        FullPathHash = fullPathHash;
        Loop = loop;
        Speed = speed;
    }
}