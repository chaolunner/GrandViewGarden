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
    private Dictionary<Type[], INetworkTimeline> timelineDict;
    private Dictionary<NetworkId, Dictionary<INetworkTimeline, TimePointWithLerp>> timePointWithLerpDict;

    public NetworkGroup(NetworkGroupData data)
    {
        networkGroupData = data;
        timelineDict = new Dictionary<Type[], INetworkTimeline>();
        timePointWithLerpDict = new Dictionary<NetworkId, Dictionary<INetworkTimeline, TimePointWithLerp>>();

        LockstepUtility.OnRestart += OnRestart;

        networkGroupData.Group.OnAdd().Subscribe(entity =>
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

    public INetworkTimeline CreateTimeline(params Type[] inputTypes)
    {
        if (!timelineDict.ContainsKey(inputTypes))
        {
            timelineDict.Add(inputTypes, new NetworkTimeline());
        }
        return timelineDict[inputTypes];
    }

    public void UpdateTimeline(IEntity entity, Type[] inputTypes, INetworkTimeline subject)
    {
        var networkIdentityComponent = entity.GetComponent<NetworkIdentityComponent>();
        var identity = networkIdentityComponent.Identity;
        if (!timePointWithLerpDict.ContainsKey(identity))
        {
            timePointWithLerpDict.Add(identity, new Dictionary<INetworkTimeline, TimePointWithLerp>());
        }
        if (!timePointWithLerpDict[identity].ContainsKey(subject))
        {
            timePointWithLerpDict[identity].Add(subject, new TimePointWithLerp());
        }
        var timePointWithLerp = timePointWithLerpDict[identity][subject];
        var beforeStep = timePointWithLerp.TickId;

        timePointWithLerp.Begin(Time.deltaTime, networkGroupData.FixedDeltaTime);

        PushUntilLastStep(entity, inputTypes, subject);

        Rollback(entity, subject);

        Forecast(beforeStep, identity, inputTypes, subject);

        ApplyTimePoint(entity, subject);

        timePointWithLerp.End();
    }

    private int PushUntilLastStep(IEntity entity, Type[] inputTypes, INetworkTimeline subject)
    {
        var networkIdentityComponent = entity.GetComponent<NetworkIdentityComponent>();
        var identity = networkIdentityComponent.Identity;
        var timePointWithLerp = timePointWithLerpDict[identity][subject];
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

    private void ApplyTimePoint(IEntity entity, INetworkTimeline subject)
    {
        var networkIdentityComponent = entity.GetComponent<NetworkIdentityComponent>();
        var identity = networkIdentityComponent.Identity;
        var timePointWithLerp = timePointWithLerpDict[identity][subject];
        var timePoints = timePointWithLerp.TimePoints;
        var totalTime = timePointWithLerp.TotalTime;
        var from = timePointWithLerp.From;
        var to = timePointWithLerp.To;
        var time = 0f;

        for (int i = 0; i < timePoints.Count; i++)
        {
            List<IUserInputResult[]> result = null;
            for (int j = 0; j < timePoints[i].Tracks.Length; j++)
            {
                if (timePoints[i].Tracks[j] == null) { continue; }
                var deltaTime = (float)timePoints[i].Tracks[j].DeltaTime;
                var p1 = time / totalTime;
                time += deltaTime;
                var p2 = time / totalTime;
                if (p1 < from && p2 > to)
                {
                    result = subject.OnNext(new TimelineData(entity, timePoints[i].Tracks, (to - from) * totalTime));
                }
                else if (p1 >= from && p1 <= to && p2 >= from && p2 <= to)
                {
                    result = subject.OnNext(new TimelineData(entity, timePoints[i].Tracks, deltaTime));
                }
                else if (p1 >= from && p1 <= to)
                {
                    result = subject.OnNext(new TimelineData(entity, timePoints[i].Tracks, (to - p1) * totalTime));
                }
                else if (p2 >= from && p2 <= to)
                {
                    result = subject.OnNext(new TimelineData(entity, timePoints[i].Tracks, (p2 - from) * totalTime));
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

    private void OnRestart()
    {
        timePointWithLerpDict.Clear();
    }

    private void Rollback(IEntity entity, INetworkTimeline subject)
    {
        var networkIdentityComponent = entity.GetComponent<NetworkIdentityComponent>();
        var identity = networkIdentityComponent.Identity;
        var timePointWithLerp = timePointWithLerpDict[identity][subject];
        if (networkGroupData.UseForecast && !timePointWithLerp.IsPlaying && timePointWithLerp.ForecastData.Count > 0 && timePointWithLerp.RealtimeData.Count > 0)
        {
            timePointWithLerp.Rollback();
        }
    }

    private void Forecast(int beforeStep, NetworkId identity, Type[] inputTypes, INetworkTimeline subject)
    {
        var timePointWithLerp = timePointWithLerpDict[identity][subject];
        if (networkGroupData.UseForecast && !timePointWithLerp.IsPlaying)
        {
            var tracks = GetUserInputDataByInputTypes(timePointWithLerp.TickId - 1, identity.UserId, inputTypes);
            var tickId = 0;
            var deltaTime = Fix64.Zero;
            for (int i = 0; i < tracks.Length; i++)
            {
                if (tracks[i] == null) { continue; }
                tickId = tracks[i].TickId;
                deltaTime = tracks[i].DeltaTime;
                break;
            }
            timePointWithLerp.Forecast(deltaTime, new TimePointData(tickId, tracks), networkGroupData.MaxForecastSteps);
        }
    }

    public void Dispose()
    {
        LockstepUtility.OnRestart -= OnRestart;
    }
}
