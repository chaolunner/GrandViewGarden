using System.Collections.Generic;
using UniEasy.ECS;

public class LockstepFactory
{
    private Dictionary<NetworkGroupData, NetworkGroup> groupDict = new Dictionary<NetworkGroupData, NetworkGroup>();
    private List<NetworkGroup> groups = new List<NetworkGroup>();

    public NetworkGroup Create(IGroup group = null, bool useForecast = LockstepSettings.UseForecast, int priority = (int)LockstepSettings.Priority.Middle, int maxForecastSteps = LockstepSettings.MaxForecastSteps, float fixedDeltaTime = LockstepSettings.FixedDeltaTime)
    {
        var data = new NetworkGroupData(group, useForecast, priority, maxForecastSteps, fixedDeltaTime);
        if (!groupDict.ContainsKey(data))
        {
            groupDict.Add(data, new NetworkGroup(data));
            groups.Add(groupDict[data]);
            groups.Sort();
        }
        return groupDict[data];
    }

    public void Update()
    {
        for (int i = 0; i < groups.Count; i++)
        {
            groups[i].Update();
        }
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
