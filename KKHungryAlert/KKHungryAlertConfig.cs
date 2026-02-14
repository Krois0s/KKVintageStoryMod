namespace KKHungryAlert;

public class KKHungryAlertConfig
{
    public float SatietyThreshold { get; set; } = 400f;
    public float CheckIntervalSeconds { get; set; } = 30f;
    public float SoundVolume { get; set; } = 0.5f;
}
