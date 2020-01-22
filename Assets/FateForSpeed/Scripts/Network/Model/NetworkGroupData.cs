using UniEasy.ECS;

public struct NetworkGroupData
{
    public IGroup Group;
    public bool UseForecast;
    public int Priority;
    public int MaxForecastSteps;
    public float FixedDeltaTime;

    public NetworkGroupData(IGroup group, bool useForecast = LockstepSettings.UseForecast, int priority = (int)LockstepSettings.Priority.Middle, int maxForecastSteps = LockstepSettings.MaxForecastSteps, float fixedDeltaTime = LockstepSettings.FixedDeltaTime)
    {
        Group = group;
        UseForecast = useForecast;
        Priority = priority;
        MaxForecastSteps = maxForecastSteps;
        FixedDeltaTime = fixedDeltaTime;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = 17;

            if (Group != null) { hashCode = (hashCode * 23) + Group.GetHashCode(); }

            hashCode = (hashCode * 23) + (UseForecast ? 1 : 0);

            hashCode = (hashCode * 23) + Priority;

            hashCode = (hashCode * 23) + MaxForecastSteps;

            hashCode = (hashCode * 23) + FixedDeltaTime.GetHashCode();

            return hashCode;
        }
    }

    public override bool Equals(object obj)
    {
        return obj is NetworkGroupData && ((NetworkGroupData)obj) == this;
    }

    public static bool operator ==(NetworkGroupData lhs, NetworkGroupData rhs)
    {
        return lhs.Group == rhs.Group && lhs.UseForecast == rhs.UseForecast && lhs.Priority == rhs.Priority && lhs.MaxForecastSteps == rhs.MaxForecastSteps && lhs.FixedDeltaTime == rhs.FixedDeltaTime;
    }

    public static bool operator !=(NetworkGroupData lhs, NetworkGroupData rhs)
    {
        return lhs.Group != rhs.Group || lhs.UseForecast != rhs.UseForecast || lhs.Priority != rhs.Priority || lhs.MaxForecastSteps != rhs.MaxForecastSteps || lhs.FixedDeltaTime != rhs.FixedDeltaTime;
    }
}
