using System.Collections.Generic;
using UniEasy.ECS;

public class LockstepFactory
{
    private Dictionary<IGroup, NetworkGroup> groupDict = new Dictionary<IGroup, NetworkGroup>();

    public NetworkGroup Create(IGroup group, bool useForecast = LockstepSettings.UseForecast, int maxForecastSteps = LockstepSettings.MaxForecastSteps, float fixedDeltaTime = LockstepSettings.FixedDeltaTime)
    {
        if (!groupDict.ContainsKey(group))
        {
            groupDict.Add(group, new NetworkGroup(group, useForecast, maxForecastSteps, fixedDeltaTime));
        }
        return groupDict[group];
    }

    public void Destroy(IGroup group)
    {
        if (groupDict.ContainsKey(group))
        {
            groupDict[group].Dispose();
            groupDict.Remove(group);
        }
    }
}
