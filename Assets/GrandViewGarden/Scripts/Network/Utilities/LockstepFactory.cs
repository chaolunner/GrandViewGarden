using System.Collections.Generic;
using UniEasy.ECS;

public class LockstepFactory
{
    private List<NetworkGroup> groups = new List<NetworkGroup>();
    private List<NetworkGroup> physicsGroups = new List<NetworkGroup>();

    public NetworkGroup Create(IGroup group = null, bool usePhysics = LockstepSettings.UsePhysics, bool useForecast = LockstepSettings.UseForecast, int priority = (int)LockstepSettings.Priority.Middle, int maxForecastSteps = LockstepSettings.MaxForecastSteps, float fixedDeltaTime = LockstepSettings.FixedDeltaTime)
    {
        var networkGroup = new NetworkGroup(group, usePhysics, useForecast, priority, maxForecastSteps, fixedDeltaTime);
        if (usePhysics)
        {
            if (!physicsGroups.Contains(networkGroup))
            {
                physicsGroups.Add(networkGroup);
                physicsGroups.Sort();
            }
            return physicsGroups.Find(g => g.Equals(networkGroup));
        }
        else
        {
            if (!groups.Contains(networkGroup))
            {
                groups.Add(networkGroup);
                groups.Sort();
            }
            return groups.Find(g => g.Equals(networkGroup));
        }
    }

    public void FixedUpdate()
    {
        for (int i = 0; i < physicsGroups.Count; i++)
        {
            physicsGroups[i].Update();
        }
    }

    public void Update()
    {
        for (int i = 0; i < groups.Count; i++)
        {
            groups[i].Update();
        }
    }

    public void Destroy(NetworkGroup networkGroup)
    {
        networkGroup.Dispose();
        if (physicsGroups.Contains(networkGroup))
        {
            physicsGroups.Remove(networkGroup);
        }
        if (groups.Contains(networkGroup))
        {
            groups.Remove(networkGroup);
        }
    }
}
