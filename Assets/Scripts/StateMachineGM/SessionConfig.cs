
public class SessionConfig
{
    public int CurrentLevel {  get; private set; }
    public float RunDurantion { get; private set; }

    private const float baseDuration = 30f;
    private const float durationIncrement = 15f;

    public SessionConfig()
    {
        Reset();
    }

    public void AdvanceRun()
    {
        CurrentLevel++;
        RunDurantion += durationIncrement;
    }

    public void Reset()
    {
        CurrentLevel = 0;
        RunDurantion = baseDuration;
    }
}

