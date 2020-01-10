using System.Collections.Generic;
using UnityEngine;
using UniEasy.ECS;
using System;
using Common;
using UniRx;

public class NetworkGroup : IDisposable
{
    public event Action OnUpdate;

    private NetworkGroupData networkGroupData;
    private TimePointWithLerp defaultTimePointWithLerp;
    private Dictionary<Type[], INetworkTimeline> timelineDict;
    private Dictionary<NetworkId, Dictionary<INetworkTimeline, TimePointWithLerp>> timePointWithLerpDict;

    public NetworkGroup(NetworkGroupData data)
    {
        networkGroupData = data;
        timelineDict = new Dictionary<Type[], INetworkTimeline>();
        timePointWithLerpDict = new Dictionary<NetworkId, Dictionary<INetworkTimeline, TimePointWithLerp>>();

        LockstepUtility.OnRestart += OnRestart;

        if (networkGroupData.Group != null)
        {
            networkGroupData.Group.OnAdd()
               .Where(entity => entity.HasComponent<NetworkIdentityComponent>())
               .Subscribe(entity =>
           {
               var networkIdentityComponent = entity.GetComponent<NetworkIdentityComponent>();

               Observable.EveryUpdate().Subscribe(_ =>
               {
                   if (networkIdentityComponent.IsLocalPlayer)
                   {
                       OnUpdate?.Invoke();
                   }
                   var e = timelineDict.GetEnumerator();
                   while (e.MoveNext())
                   {
                       UpdateTimeline(entity, e.Current.Key, e.Current.Value);
                   }
               }).AddTo(networkIdentityComponent.Disposer);
           });
        }
        else
        {
            Observable.EveryUpdate().Subscribe(_ =>
            {
                OnUpdate?.Invoke();
                var e = timelineDict.GetEnumerator();
                while (e.MoveNext())
                {
                    UpdateTimeline(e.Current.Key, e.Current.Value);
                }
            });
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

        timePointWithLerp.Begin(Time.deltaTime, networkGroupData.FixedDeltaTime);

        PushUntilLastStep(entity, inputTypes, timeline);

        Rollback(entity, timeline);

        Forecast(identity, inputTypes, timeline);

        ApplyTimePoint(entity, timeline);

        timePointWithLerp.End();
    }

    private void UpdateTimeline(Type[] inputTypes, INetworkTimeline timeline)
    {
        if (defaultTimePointWithLerp == null)
        {
            defaultTimePointWithLerp = new TimePointWithLerp();
        }

        defaultTimePointWithLerp.Begin(Time.deltaTime, networkGroupData.FixedDeltaTime);

        PushUntilLastStep(inputTypes);

        Rollback(timeline);

        Forecast(inputTypes);

        ApplyTimePoint(timeline);

        defaultTimePointWithLerp.End();
    }

    private int PushUntilLastStep(IEntity entity, Type[] inputTypes, INetworkTimeline timeline)
    {
        var networkIdentityComponent = entity.GetComponent<NetworkIdentityComponent>();
        var identity = networkIdentityComponent.Identity;
        var timePointWithLerp = timePointWithLerpDict[identity][timeline];
        var tickId = timePointWithLerp.TickId;
        if (!timePointWithLerp.IsPlaying)
        {
            var index = 0;
            while (index < inputTypes.Length)
            {
                var userInputData = LockstepUtility.GetUserInputData(tickId, identity.UserId, inputTypes[index]);
                if (userInputData != null)
                {
                    var tracks = GetUserInputDataByInputTypes(tickId, identity.UserId, inputTypes);
                    timePointWithLerp.AddRealtimeData(userInputData.DeltaTime, new TimePointData(userInputData.TickId, tracks));
                    index = 0;
                    tickId++;
                }
                else
                {
                    index++;
                }
            }
        }
        timePointWithLerp.TickId = tickId;
        return tickId;
    }

    private UserInputData[] GetUserInputDataByInputTypes(int tickId, int userId, Type[] inputTypes)
    {
        var data = new UserInputData[inputTypes.Length];
        for (int i = 0; i < inputTypes.Length; i++)
        {
            data[i] = LockstepUtility.GetUserInputData(tickId, userId, inputTypes[i]);
        }
        return data;
    }

    private int PushUntilLastStep(Type[] inputTypes)
    {
        var tickId = defaultTimePointWithLerp.TickId;
        if (!defaultTimePointWithLerp.IsPlaying)
        {
            while (LockstepUtility.HasTickId(tickId))
            {
                var tracks = GetUserInputDataByInputTypes(tickId, inputTypes);
                defaultTimePointWithLerp.AddRealtimeData(LockstepUtility.GetDeltaTime(tickId), new TimePointData(tickId, tracks));
                tickId++;
            }
        }
        defaultTimePointWithLerp.TickId = tickId;
        return tickId;
    }

    private UserInputData[][] GetUserInputDataByInputTypes(int tickId, Type[] inputTypes)
    {
        var userInputData = LockstepUtility.GetAllUserInputData(tickId);
        var dataDict = new Dictionary<Type, List<UserInputData>>();

        for (int i = 0; i < userInputData.Length; i++)
        {
            var type = userInputData[i].Input.GetType();
            if (!dataDict.ContainsKey(type))
            {
                dataDict.Add(type, new List<UserInputData>());
            }
            dataDict[type].Add(userInputData[i]);
        }

        var data = new UserInputData[inputTypes.Length][];
        for (int i = 0; i < inputTypes.Length; i++)
        {
            if (dataDict.ContainsKey(inputTypes[i]))
            {
                data[i] = new UserInputData[dataDict[inputTypes[i]].Count];
                for (int j = 0; j < dataDict[inputTypes[i]].Count; j++)
                {
                    data[i][j] = dataDict[inputTypes[i]][j];
                }
            }
            else
            {
                data[i] = new UserInputData[0];
            }
        }

        return data;
    }

    private void ApplyTimePoint(IEntity entity, INetworkTimeline timeline)
    {
        var networkIdentityComponent = entity.GetComponent<NetworkIdentityComponent>();
        var identity = networkIdentityComponent.Identity;
        var timePointWithLerp = timePointWithLerpDict[identity][timeline];
        var timePoints = timePointWithLerp.TimePoints;
        var totalTime = timePointWithLerp.TotalTime;
        var from = timePointWithLerp.From;
        var to = timePointWithLerp.To;
        var time = 0f;

        for (int i = 0; i < timePoints.Count; i++)
        {
            List<IUserInputResult[]> result = null;
            if (networkIdentityComponent.TickIdWhenCreated < 0 || networkIdentityComponent.TickIdWhenCreated > timePoints[i].TickId) { continue; }
            for (int j = 0; j < timePoints[i].Tracks[0].Length; j++)
            {
                if (timePoints[i].Tracks[0][j] == null) { continue; }
                var deltaTime = (float)timePoints[i].Tracks[0][j].DeltaTime;
                var p1 = time / totalTime;
                time += deltaTime;
                var p2 = time / totalTime;
                if (p1 < from && p2 > to)
                {
                    result = timeline.Forward(new ForwardTimelineData(entity, timePoints[i], (to - from) * totalTime));
                }
                else if (p1 >= from && p1 <= to && p2 >= from && p2 <= to)
                {
                    result = timeline.Forward(new ForwardTimelineData(entity, timePoints[i], deltaTime));
                }
                else if (p1 >= from && p1 <= to)
                {
                    result = timeline.Forward(new ForwardTimelineData(entity, timePoints[i], (to - p1) * totalTime));
                }
                else if (p2 >= from && p2 <= to)
                {
                    result = timeline.Forward(new ForwardTimelineData(entity, timePoints[i], (p2 - from) * totalTime));
                }
                break;
            }
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
        var timePoints = defaultTimePointWithLerp.TimePoints;
        var totalTime = defaultTimePointWithLerp.TotalTime;
        var from = defaultTimePointWithLerp.From;
        var to = defaultTimePointWithLerp.To;
        var time = 0f;

        for (int i = 0; i < timePoints.Count; i++)
        {
            List<IUserInputResult[]> result = null;
            for (int j = 0; j < timePoints[i].Tracks[0].Length; j++)
            {
                if (timePoints[i].Tracks[0][j] == null) { continue; }
                var deltaTime = (float)timePoints[i].Tracks[0][j].DeltaTime;
                var p1 = time / totalTime;
                time += deltaTime;
                var p2 = time / totalTime;
                if (p1 < from && p2 > to)
                {
                    result = timeline.Forward(new ForwardTimelineData(null, timePoints[i], (to - from) * totalTime));
                }
                else if (p1 >= from && p1 <= to && p2 >= from && p2 <= to)
                {
                    result = timeline.Forward(new ForwardTimelineData(null, timePoints[i], deltaTime));
                }
                else if (p1 >= from && p1 <= to)
                {
                    result = timeline.Forward(new ForwardTimelineData(null, timePoints[i], (to - p1) * totalTime));
                }
                else if (p2 >= from && p2 <= to)
                {
                    result = timeline.Forward(new ForwardTimelineData(null, timePoints[i], (p2 - from) * totalTime));
                }
                break;
            }
            if (result != null && timePoints[i].ForecastCount > 0)
            {
                for (int j = 0; j < result.Count; j++)
                {
                    defaultTimePointWithLerp.RollbackData.Add(result[j]);
                }
            }
        }
    }

    private void OnRestart()
    {
        timePointWithLerpDict.Clear();
        defaultTimePointWithLerp = null;
    }

    private void Rollback(IEntity entity, INetworkTimeline timeline)
    {
        var networkIdentityComponent = entity.GetComponent<NetworkIdentityComponent>();
        var identity = networkIdentityComponent.Identity;
        var timePointWithLerp = timePointWithLerpDict[identity][timeline];
        if (networkGroupData.UseForecast && !timePointWithLerp.IsPlaying && timePointWithLerp.ForecastData.Count > 0 && timePointWithLerp.RealtimeData.Count > 0)
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
        if (networkGroupData.UseForecast && !defaultTimePointWithLerp.IsPlaying && defaultTimePointWithLerp.ForecastData.Count > 0 && defaultTimePointWithLerp.RealtimeData.Count > 0)
        {
            for (int i = defaultTimePointWithLerp.RollbackData.Count - 1; i >= 0; i--)
            {
                timeline.Reverse(new ReverseTimelineData(null, defaultTimePointWithLerp.RollbackData[i]));
            }
            defaultTimePointWithLerp.RollbackData.Clear();
        }
    }

    private void Forecast(NetworkId identity, Type[] inputTypes, INetworkTimeline timeline)
    {
        var timePointWithLerp = timePointWithLerpDict[identity][timeline];
        if (networkGroupData.UseForecast && !timePointWithLerp.IsPlaying)
        {
            var tickId = timePointWithLerp.TickId - 1;
            if (LockstepUtility.HasTickId(tickId))
            {
                var tracks = GetUserInputDataByInputTypes(tickId, identity.UserId, inputTypes);
                var deltaTime = LockstepUtility.GetDeltaTime(tickId);
                timePointWithLerp.Forecast(deltaTime, new TimePointData(tickId, tracks), networkGroupData.MaxForecastSteps);
            }
        }
    }

    private void Forecast(Type[] inputTypes)
    {
        if (networkGroupData.UseForecast && !defaultTimePointWithLerp.IsPlaying)
        {
            var tickId = defaultTimePointWithLerp.TickId - 1;
            if (LockstepUtility.HasTickId(tickId))
            {
                var tracks = GetUserInputDataByInputTypes(tickId, inputTypes);
                var deltaTime = LockstepUtility.GetDeltaTime(tickId);
                defaultTimePointWithLerp.Forecast(deltaTime, new TimePointData(tickId, tracks), networkGroupData.MaxForecastSteps);
            }
        }
    }

    public void Dispose()
    {
        LockstepUtility.OnRestart -= OnRestart;
    }
}
