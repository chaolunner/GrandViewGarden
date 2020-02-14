using System.Collections.Generic;
using UnityEngine;
using UniEasy.ECS;
using System;
using Common;

public class NetworkGroup : IDisposable, IComparable<NetworkGroup>
{
    public IGroup Group;
    public bool UsePhysics;
    public bool UseForecast;
    public int Priority;
    public int MaxForecastSteps;
    public float FixedDeltaTime;

    public event Action OnUpdate;

    private Dictionary<Type[], INetworkTimeline> timelineDict;
    private Dictionary<NetworkId, Dictionary<INetworkTimeline, TimePointWithLerp>> timePointWithLerpDict;
    private Dictionary<INetworkTimeline, TimePointWithLerp> defaultTimePointWithLerpDict;

    public NetworkGroup(IGroup group, bool usePhysics = LockstepSettings.UsePhysics, bool useForecast = LockstepSettings.UseForecast, int priority = (int)LockstepSettings.Priority.Middle, int maxForecastSteps = LockstepSettings.MaxForecastSteps, float fixedDeltaTime = LockstepSettings.FixedDeltaTime)
    {
        Group = group;
        UsePhysics = usePhysics;
        UseForecast = useForecast;
        Priority = priority;
        MaxForecastSteps = maxForecastSteps;
        FixedDeltaTime = fixedDeltaTime;

        timelineDict = new Dictionary<Type[], INetworkTimeline>();
        timePointWithLerpDict = new Dictionary<NetworkId, Dictionary<INetworkTimeline, TimePointWithLerp>>();
        defaultTimePointWithLerpDict = new Dictionary<INetworkTimeline, TimePointWithLerp>();

        LockstepUtility.OnRestart += OnRestart;
    }

    public void Update()
    {
        if (Group != null)
        {
            for (int i = 0; i < Group.Entities.Count; i++)
            {
                if (Group.Entities[i] == null) { continue; }
                var entity = Group.Entities[i];
                if (!entity.HasComponent<NetworkIdentityComponent>()) { continue; }
                var networkIdentityComponent = entity.GetComponent<NetworkIdentityComponent>();
                if (networkIdentityComponent.IsLocalPlayer)
                {
                    OnUpdate?.Invoke();
                }
                var e = timelineDict.GetEnumerator();
                while (e.MoveNext())
                {
                    UpdateTimeline(entity, e.Current.Key, e.Current.Value);
                }
            }
        }
        else
        {
            OnUpdate?.Invoke();
            var e = timelineDict.GetEnumerator();
            while (e.MoveNext())
            {
                UpdateTimeline(e.Current.Key, e.Current.Value);
            }
        }
    }

    public INetworkTimeline CreateTimeline(params Type[] inputTypes)
    {
        if (!timelineDict.ContainsKey(inputTypes))
        {
            timelineDict.Add(inputTypes, new NetworkTimeline());
        }
        return timelineDict[inputTypes];
    }

    private void UpdateTimeline(IEntity entity, Type[] inputTypes, INetworkTimeline timeline)
    {
        var networkIdentityComponent = entity.GetComponent<NetworkIdentityComponent>();
        var identity = networkIdentityComponent.Identity;
        if (!timePointWithLerpDict.ContainsKey(identity))
        {
            timePointWithLerpDict.Add(identity, new Dictionary<INetworkTimeline, TimePointWithLerp>());
        }
        if (!timePointWithLerpDict[identity].ContainsKey(timeline))
        {
            timePointWithLerpDict[identity].Add(timeline, new TimePointWithLerp());
        }
        var timePointWithLerp = timePointWithLerpDict[identity][timeline];

        timePointWithLerp.Begin(Time.deltaTime, FixedDeltaTime);

        PushUntilLastStep(entity, inputTypes, timeline);

        Rollback(entity, timeline);

        Forecast(identity, inputTypes, timeline);

        ApplyTimePoint(entity, timeline);

        timePointWithLerp.End();
    }

    private void UpdateTimeline(Type[] inputTypes, INetworkTimeline timeline)
    {
        if (!defaultTimePointWithLerpDict.ContainsKey(timeline))
        {
            defaultTimePointWithLerpDict.Add(timeline, new TimePointWithLerp());
        }
        var timePointWithLerp = defaultTimePointWithLerpDict[timeline];

        timePointWithLerp.Begin(Time.deltaTime, FixedDeltaTime);

        PushUntilLastStep(inputTypes, timeline);

        Rollback(timeline);

        Forecast(inputTypes, timeline);

        ApplyTimePoint(timeline);

        timePointWithLerp.End();
    }

    private int PushUntilLastStep(IEntity entity, Type[] inputTypes, INetworkTimeline timeline)
    {
        var networkIdentityComponent = entity.GetComponent<NetworkIdentityComponent>();
        var identity = networkIdentityComponent.Identity;
        var timePointWithLerp = timePointWithLerpDict[identity][timeline];
        var tickId = timePointWithLerp.TickId;
        if (tickId < networkIdentityComponent.TickIdWhenCreated)
        {
            tickId = networkIdentityComponent.TickIdWhenCreated;
        }
        if (!timePointWithLerp.IsPlaying)
        {
            while (LockstepUtility.HasTickId(tickId))
            {
                var userInputData = GetUserInputDataByInputTypes(tickId, identity.UserId, inputTypes);
                for (int i = 0; i < userInputData.Length; i++)
                {
                    timePointWithLerp.AddRealtimeData(new TimePointData(tickId, LockstepUtility.GetDeltaTime(tickId), userInputData[i]));
                }
                tickId++;
            }
        }
        timePointWithLerp.TickId = tickId;
        return tickId;
    }

    private UserInputData[][] GetUserInputDataByInputTypes(int tickId, int userId, Type[] inputTypes)
    {
        var dataList = new List<UserInputData[]>();
        for (int i = 0; i < inputTypes.Length; i++)
        {
            var userInputData = LockstepUtility.GetUserInputData(tickId, userId, inputTypes[i]);
            if (userInputData == null) { continue; }
            for (int j = 0; j < userInputData.Length; j++)
            {
                while (j >= dataList.Count)
                {
                    dataList.Add(new UserInputData[inputTypes.Length]);
                }
                dataList[j][i] = userInputData[j];
            }
        }
        if (dataList.Count <= 0)
        {
            var data = new UserInputData[inputTypes.Length];
            for (int i = 0; i < inputTypes.Length; i++)
            {
                data[i] = LockstepUtility.CreateUesrInputData(tickId);
            }
            dataList.Add(data);
        }
        return dataList.ToArray();
    }

    private int PushUntilLastStep(Type[] inputTypes, INetworkTimeline timeline)
    {
        var timePointWithLerp = defaultTimePointWithLerpDict[timeline];
        var tickId = timePointWithLerp.TickId;
        if (!timePointWithLerp.IsPlaying)
        {
            while (LockstepUtility.HasTickId(tickId))
            {
                var userInputData = GetUserInputDataByInputTypes(tickId, inputTypes);
                for (int i = 0; i < userInputData.Length; i++)
                {
                    timePointWithLerp.AddRealtimeData(new TimePointData(tickId, LockstepUtility.GetDeltaTime(tickId), userInputData[i]));
                }
                tickId++;
            }
        }
        timePointWithLerp.TickId = tickId;
        return tickId;
    }

    private UserInputData[][][] GetUserInputDataByInputTypes(int tickId, Type[] inputTypes)
    {
        var dataList = new List<UserInputData[][]>();
        for (int i = 0; i < inputTypes.Length; i++)
        {
            var userInputData = LockstepUtility.GetAllUserInputData(tickId, inputTypes[i]);
            if (userInputData == null) { continue; }
            for (int j = 0; j < userInputData.Length; j++)
            {
                while (j >= dataList.Count)
                {
                    dataList.Add(new UserInputData[inputTypes.Length][]);
                }
                dataList[j][i] = userInputData[j];
            }
        }
        if (dataList.Count <= 0)
        {
            var data = new UserInputData[inputTypes.Length][];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new UserInputData[1] { LockstepUtility.CreateUesrInputData(tickId) };
            }
            dataList.Add(data);
        }
        return dataList.ToArray();
    }

    private void ApplyTimePoint(IEntity entity, INetworkTimeline timeline)
    {
        var networkIdentityComponent = entity.GetComponent<NetworkIdentityComponent>();
        var identity = networkIdentityComponent.Identity;
        var timePointWithLerp = timePointWithLerpDict[identity][timeline];
        var timePoints = timePointWithLerp.TimePoints;
        var from = timePointWithLerp.From;
        var to = timePointWithLerp.To;

        for (int i = 0; i < timePoints.Count; i++)
        {
            if (networkIdentityComponent.TickIdWhenCreated < 0 || networkIdentityComponent.TickIdWhenCreated > timePoints[i].TickId) { continue; }
            var duration = timePoints[i].Duration;
            var result = DoForward(timeline, entity, timePoints[i], from, to, duration, i / (float)timePoints.Count);
            if (result != null && timePoints[i].ForecastCount > 0)
            {
                for (int j = 0; j < result.Count; j++)
                {
                    timePointWithLerp.RollbackData.Add(result[j]);
                }
            }
        }
    }

    private void ApplyTimePoint(INetworkTimeline timeline)
    {
        var timePointWithLerp = defaultTimePointWithLerpDict[timeline];
        var timePoints = timePointWithLerp.TimePoints;
        var from = timePointWithLerp.From;
        var to = timePointWithLerp.To;

        for (int i = 0; i < timePoints.Count; i++)
        {
            var duration = timePoints[i].Duration;
            var result = DoForward(timeline, null, timePoints[i], from, to, duration, i / (float)timePoints.Count);
            if (result != null && timePoints[i].ForecastCount > 0)
            {
                for (int j = 0; j < result.Count; j++)
                {
                    timePointWithLerp.RollbackData.Add(result[j]);
                }
            }
        }
    }

    private List<IUserInputResult[]> DoForward(INetworkTimeline timeline, IEntity entity, TimePointData timePoint, float from, float to, Fix64 deltaTime, float t)
    {
        if (t >= from && t <= to)
        {
            timePoint.DeltaTime = deltaTime;
            return timeline.Forward(new ForwardTimelineData(entity, timePoint));
        }
        return null;
    }

    private void OnRestart()
    {
        timePointWithLerpDict.Clear();
        defaultTimePointWithLerpDict.Clear();
    }

    private void Rollback(IEntity entity, INetworkTimeline timeline)
    {
        var networkIdentityComponent = entity.GetComponent<NetworkIdentityComponent>();
        var identity = networkIdentityComponent.Identity;
        var timePointWithLerp = timePointWithLerpDict[identity][timeline];
        if (UseForecast && !timePointWithLerp.IsPlaying && timePointWithLerp.ForecastData.Count > 0 && timePointWithLerp.RealtimeData.Count > 0)
        {
            for (int i = timePointWithLerp.RollbackData.Count - 1; i >= 0; i--)
            {
                timeline.Reverse(new ReverseTimelineData(entity, timePointWithLerp.RollbackData[i]));
            }
            timePointWithLerp.RollbackData.Clear();
        }
    }

    private void Rollback(INetworkTimeline timeline)
    {
        var timePointWithLerp = defaultTimePointWithLerpDict[timeline];
        if (UseForecast && !timePointWithLerp.IsPlaying && timePointWithLerp.ForecastData.Count > 0 && timePointWithLerp.RealtimeData.Count > 0)
        {
            for (int i = timePointWithLerp.RollbackData.Count - 1; i >= 0; i--)
            {
                timeline.Reverse(new ReverseTimelineData(null, timePointWithLerp.RollbackData[i]));
            }
            timePointWithLerp.RollbackData.Clear();
        }
    }

    private void Forecast(NetworkId identity, Type[] inputTypes, INetworkTimeline timeline)
    {
        var timePointWithLerp = timePointWithLerpDict[identity][timeline];
        if (UseForecast && !timePointWithLerp.IsPlaying)
        {
            var tickId = timePointWithLerp.TickId - 1;
            if (LockstepUtility.HasTickId(tickId))
            {
                var deltaTime = LockstepUtility.GetDeltaTime(tickId);
                var userInputData = GetUserInputDataByInputTypes(tickId, identity.UserId, inputTypes);
                for (int i = 0; i < userInputData.Length; i++)
                {
                    timePointWithLerp.Forecast(new TimePointData(tickId, deltaTime, userInputData[i]), MaxForecastSteps);
                }
            }
        }
    }

    private void Forecast(Type[] inputTypes, INetworkTimeline timeline)
    {
        var timePointWithLerp = defaultTimePointWithLerpDict[timeline];
        if (UseForecast && !timePointWithLerp.IsPlaying)
        {
            var tickId = timePointWithLerp.TickId - 1;
            if (LockstepUtility.HasTickId(tickId))
            {
                var deltaTime = LockstepUtility.GetDeltaTime(tickId);
                var userInputData = GetUserInputDataByInputTypes(tickId, inputTypes);
                for (int i = 0; i < userInputData.Length; i++)
                {
                    timePointWithLerp.Forecast(new TimePointData(tickId, deltaTime, userInputData[i]), MaxForecastSteps);
                }
            }
        }
    }

    public void Dispose()
    {
        LockstepUtility.OnRestart -= OnRestart;
    }

    public int CompareTo(NetworkGroup other)
    {
        if (Priority > other.Priority) { return 1; }
        else if (Priority < other.Priority) { return -1; }
        return 0;
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
        return obj is NetworkGroup && ((NetworkGroup)obj) == this;
    }

    public static bool operator ==(NetworkGroup lhs, NetworkGroup rhs)
    {
        return lhs.Group == rhs.Group && lhs.UseForecast == rhs.UseForecast && lhs.Priority == rhs.Priority && lhs.MaxForecastSteps == rhs.MaxForecastSteps && lhs.FixedDeltaTime == rhs.FixedDeltaTime;
    }

    public static bool operator !=(NetworkGroup lhs, NetworkGroup rhs)
    {
        return lhs.Group != rhs.Group || lhs.UseForecast != rhs.UseForecast || lhs.Priority != rhs.Priority || lhs.MaxForecastSteps != rhs.MaxForecastSteps || lhs.FixedDeltaTime != rhs.FixedDeltaTime;
    }
}
