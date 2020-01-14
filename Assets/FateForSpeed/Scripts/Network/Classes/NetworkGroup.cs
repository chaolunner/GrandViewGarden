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
        if (tickId < networkIdentityComponent.TickIdWhenCreated)
        {
            tickId = networkIdentityComponent.TickIdWhenCreated;
        }
        if (!timePointWithLerp.IsPlaying)
        {
            var index = 0;
            while (index < inputTypes.Length)
            {
                if (LockstepUtility.HasTickId(tickId))
                {
                    var userInputData = GetUserInputDataByInputTypes(tickId, identity.UserId, inputTypes);
                    for (int i = 0; i < userInputData.Length; i++)
                    {
                        timePointWithLerp.AddRealtimeData(LockstepUtility.GetDeltaTime(tickId), new TimePointData(tickId, userInputData[i]));
                    }
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

    private UserInputData[][] GetUserInputDataByInputTypes(int tickId, int userId, Type[] inputTypes)
    {
        var dataList = new List<UserInputData[]>();
        for (int i = 0; i < inputTypes.Length; i++)
        {
            var userInputData = LockstepUtility.GetUserInputData(tickId, userId, inputTypes[i]);
            for (int j = 0; j < userInputData.Length; j++)
            {
                while (j >= dataList.Count)
                {
                    dataList.Add(new UserInputData[inputTypes.Length]);
                }
                dataList[j][i] = userInputData[j];
            }
        }
        return dataList.ToArray();
    }

    private int PushUntilLastStep(Type[] inputTypes)
    {
        var tickId = defaultTimePointWithLerp.TickId;
        if (!defaultTimePointWithLerp.IsPlaying)
        {
            while (LockstepUtility.HasTickId(tickId))
            {
                var userInputData = GetUserInputDataByInputTypes(tickId, inputTypes);
                for (int i = 0; i < userInputData.Length; i++)
                {
                    defaultTimePointWithLerp.AddRealtimeData(LockstepUtility.GetDeltaTime(tickId), new TimePointData(tickId, userInputData[i]));
                }
                tickId++;
            }
        }
        defaultTimePointWithLerp.TickId = tickId;
        return tickId;
    }

    private UserInputData[][][] GetUserInputDataByInputTypes(int tickId, Type[] inputTypes)
    {
        var dataList = new List<UserInputData[][]>();
        for (int i = 0; i < inputTypes.Length; i++)
        {
            var userInputData = LockstepUtility.GetAllUserInputData(tickId, inputTypes[i]);
            for (int j = 0; j < userInputData.Length; j++)
            {
                while (j >= dataList.Count)
                {
                    dataList.Add(new UserInputData[inputTypes.Length][]);
                }
                dataList[j][i] = userInputData[j];
            }
        }
        return dataList.ToArray();
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
            for (int j = 0; j < timePoints[i].UserInputData[0].Length; j++)
            {
                if (timePoints[i].UserInputData[0][j] == null) { continue; }
                var deltaTime = (float)timePoints[i].UserInputData[0][j].DeltaTime;
                var start = time / totalTime;
                time += deltaTime;
                var end = time / totalTime;
                result = DoForward(timeline, entity, timePoints[i], from, to, start, end, deltaTime, totalTime);
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
            for (int j = 0; j < timePoints[i].UserInputData[0].Length; j++)
            {
                if (timePoints[i].UserInputData[0][j] == null) { continue; }
                var deltaTime = (float)timePoints[i].UserInputData[0][j].DeltaTime;
                var start = time / totalTime;
                time += deltaTime;
                var end = time / totalTime;
                result = DoForward(timeline, null, timePoints[i], from, to, start, end, deltaTime, totalTime);
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

    private List<IUserInputResult[]> DoForward(INetworkTimeline timeline, IEntity entity, TimePointData timePoint, float from, float to, float start, float end, float deltaTime, float totalTime)
    {
        timePoint.Start = start;
        timePoint.End = end;
        timePoint.DeltaTime = 0;
        if (start < from && end > to)
        {
            timePoint.DeltaTime = (to - from) * totalTime;
        }
        else if (start >= from && start <= to && end >= from && end <= to)
        {
            timePoint.DeltaTime = deltaTime;
        }
        else if (start >= from && start <= to)
        {
            timePoint.DeltaTime = (to - start) * totalTime;
        }
        else if (end >= from && end <= to)
        {
            timePoint.DeltaTime = (end - from) * totalTime;
        }
        if (timePoint.DeltaTime > 0)
        {
            return timeline.Forward(new ForwardTimelineData(entity, timePoint));
        }
        return null;
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
                var deltaTime = LockstepUtility.GetDeltaTime(tickId);
                var userInputData = GetUserInputDataByInputTypes(tickId, identity.UserId, inputTypes);
                for (int i = 0; i < userInputData.Length; i++)
                {
                    timePointWithLerp.Forecast(deltaTime, new TimePointData(tickId, userInputData[i]), networkGroupData.MaxForecastSteps);
                }
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
                var deltaTime = LockstepUtility.GetDeltaTime(tickId);
                var userInputData = GetUserInputDataByInputTypes(tickId, inputTypes);
                for (int i = 0; i < userInputData.Length; i++)
                {
                    defaultTimePointWithLerp.Forecast(deltaTime, new TimePointData(tickId, userInputData[i]), networkGroupData.MaxForecastSteps);
                }
            }
        }
    }

    public void Dispose()
    {
        LockstepUtility.OnRestart -= OnRestart;
    }
}
