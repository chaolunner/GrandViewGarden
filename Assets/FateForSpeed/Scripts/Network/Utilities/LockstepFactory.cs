using System.Collections.Generic;
using UniEasy.ECS;

public class LockstepFactory
{
    private Dictionary<NetworkGroupData, NetworkGroup> groupDict = new Dictionary<NetworkGroupData, NetworkGroup>();

    public NetworkGroup Create(IGroup group, bool useForecast = LockstepSettings.UseForecast, int maxForecastSteps = LockstepSettings.MaxForecastSteps, float fixedDeltaTime = LockstepSettings.FixedDeltaTime)
    {
        var data = new NetworkGroupData(group, useForecast, maxForecastSteps, fixedDeltaTime);
        if (!groupDict.ContainsKey(data))
        {
            groupDict.Add(data, new NetworkGroup(data));
        }
        return groupDict[data];
    }

    public void Destroy(NetworkGroupData data)
    {
        if (groupDict.ContainsKey(data))
        {
            groupDict[data].Dispose();
            groupDict.Remove(data);
        }
    }
}
