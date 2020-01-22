public class LockstepSettings
{
    public const bool UseForecast = false;
    public const int MaxForecastSteps = 50;
    public const float FixedDeltaTime = 0.1f;

    public enum Priority
    {
        High = 1,
        Middle = 0,
        Low = -1,
        Debug = -2,
    }
}
