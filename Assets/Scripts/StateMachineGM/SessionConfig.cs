
public class SessionConfig
{
    public int CurrentLevel {  get; private set; }
    public float RunDurantion { get; private set; }

    private const float baseDuration = 30;
    private const float durationIncrement = 15;

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

