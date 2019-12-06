using System.Collections.Generic;
using UnityEngine;
using UniEasy.ECS;
using System;
using Common;
using UniRx;

public class LockstepSystemBehaviour : NetworkSystemBehaviour
{
    public bool UseGlobalSettings = true;
    [SerializeField] private bool useForecast = true;
    [SerializeField] private int maxForecastSteps = 10;
    [SerializeField] private float fixedDeltaTime = 0.1f;
    private IGroup NetworkPlayerComponents;
    private List<Type[]> timelines = new List<Type[]>();
    private Dictionary<int, Dictionary<Type[], TimePointWithLerp>> timePointWithLerpDict = new Dictionary<int, Dictionary<Type[], TimePointWithLerp>>();

    public bool UseForecast
    {
        get
        {
            if (UseGlobalSettings) { return LockstepSettings.UseForecast; }
            return useForecast;
        }
    }

    public int MaxForecastSteps
    {
        get
        {
            if (UseGlobalSettings) { return LockstepSettings.MaxForecastSteps; }
            return maxForecastSteps;
        }
    }

    public float FixedDeltaTime
    {
        get
        {
            if (UseGlobalSettings) { return LockstepSettings.FixedDeltaTime; }
            return fixedDeltaTime;
        }
    }

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);
        NetworkPlayerComponents = this.Create(typeof(NetworkPlayerComponent));
    }

    public override void OnEnable()
    {
        base.OnEnable();
        LockstepUtility.OnRestart += OnRestart;

        NetworkPlayerComponents.OnAdd().Subscribe(entity =>
        {
            var networkPlayerComponent = entity.GetComponent<NetworkPlayerComponent>();
            var userId = networkPlayerComponent.UserId;
            IInput[] inputs;

            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (networkPlayerComponent.IsLocalPlayer)
                {
                    inputs = UpdateInputs();
                    if (inputs != null && inputs.Length > 0)
                    {
                        for (int i = 0; i < inputs.Length; i++)
                        {
                            LockstepUtility.AddInput(inputs[i]);
                        }
                    }
                }

                for (int i = 0; i < timelines.Count; i++)
                {
                    UpdateTimeline(entity, timelines[i]);
                }
            }).AddTo(this.Disposer).AddTo(networkPlayerComponent.Disposer);
        }).AddTo(this.Disposer);
    }

    public virtual IInput[] UpdateInputs() { return null; }

    public void CreateTimeline(params Type[] inputTypes)
    {
        if (!timelines.Contains(inputTypes))
        {
            timelines.Add(inputTypes);
        }
    }

    public void UpdateTimeline(IEntity entity, Type[] inputTypes)
    {
        var networkPlayerComponent = entity.GetComponent<NetworkPlayerComponent>();
        var userId = networkPlayerComponent.UserId;
        if (!timePointWithLerpDict.ContainsKey(userId))
        {
            timePointWithLerpDict.Add(userId, new Dictionary<Type[], TimePointWithLerp>());
        }
        if (!timePointWithLerpDict[userId].ContainsKey(inputTypes))
        {
            timePointWithLerpDict[userId].Add(inputTypes, new TimePointWithLerp());
        }
        var timePointWithLerp = timePointWithLerpDict[userId][inputTypes];
        var beforeStep = timePointWithLerp.TickId;

        timePointWithLerp.Begin(Time.deltaTime, FixedDeltaTime);

        PushUntilLastStep(entity, inputTypes);

        Rollback(entity, inputTypes);

        Forecast(beforeStep, userId, inputTypes);

        ApplyTimePoint(entity, inputTypes);

        timePointWithLerp.End();
    }

    private int PushUntilLastStep(IEntity entity, Type[] inputTypes)
    {
        var networkPlayerComponent = entity.GetComponent<NetworkPlayerComponent>();
        var userId = networkPlayerComponent.UserId;
        var timePointWithLerp = timePointWithLerpDict[userId][inputTypes];
        var tickId = timePointWithLerp.TickId;
        if (!timePointWithLerp.IsPlaying)
        {
            var index = 0;
            while (index < inputTypes.Length)
            {
                var userInputData = LockstepUtility.GetUserInputData(tickId, userId, inputTypes[index]);
                if (userInputData != null)
                {
                    var tracks = GetUserInputDataByInputTypes(tickId, userId, inputTypes);
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

    private void ApplyTimePoint(IEntity entity, Type[] inputTypes)
    {
        var networkPlayerComponent = entity.GetComponent<NetworkPlayerComponent>();
        var userId = networkPlayerComponent.UserId;
        var timePointWithLerp = timePointWithLerpDict[userId][inputTypes];
        var timePoints = timePointWithLerp.TimePoints;
        var totalTime = timePointWithLerp.TotalTime;
        var from = timePointWithLerp.From;
        var to = timePointWithLerp.To;
        var time = 0f;

        for (int i = 0; i < timePoints.Count; i++)
        {
            IUserInputResult[] result = null;
            for (int j = 0; j < timePoints[i].Tracks.Length; j++)
            {
                if (timePoints[i].Tracks[j] == null) { continue; }
                var deltaTime = (float)timePoints[i].Tracks[j].DeltaTime;
                var p1 = time / totalTime;
                time += deltaTime;
                var p2 = time / totalTime;
                if (p1 < from && p2 > to)
                {
                    result = ApplyUserInput(entity, timePoints[i].Tracks, (to - from) * totalTime);
                }
                else if (p1 >= from && p1 <= to && p2 >= from && p2 <= to)
                {
                    result = ApplyUserInput(entity, timePoints[i].Tracks, deltaTime * totalTime);
                }
                else if (p1 >= from && p1 <= to)
                {
                    result = ApplyUserInput(entity, timePoints[i].Tracks, (to - p1) * totalTime);
                }
                else if (p2 >= from && p2 <= to)
                {
                    result = ApplyUserInput(entity, timePoints[i].Tracks, (p2 - from) * totalTime);
                }
                break;
            }
            if (result != null)
            {
                for (int j = 0; j < result.Length; j++)
                {
                    result[j].Execute();
                }
                if (timePoints[i].ForecastCount > 0)
                {
                    timePointWithLerp.RollbackData.Add(result);
                }
            }
        }
    }

    public virtual IUserInputResult[] ApplyUserInput(IEntity entity, UserInputData[] userInputData, float deltaTime)
    {
        throw new NotImplementedException();
    }

    public virtual void OnRestart()
    {
        timePointWithLerpDict.Clear();
    }

    private void Rollback(IEntity entity, Type[] inputTypes)
    {
        var networkPlayerComponent = entity.GetComponent<NetworkPlayerComponent>();
        var userId = networkPlayerComponent.UserId;
        var timePointWithLerp = timePointWithLerpDict[userId][inputTypes];
        if (UseForecast && !timePointWithLerp.IsPlaying && timePointWithLerp.ForecastData.Count > 0 && timePointWithLerp.RealtimeData.Count > 0)
        {
            timePointWithLerp.Rollback();
        }
    }

    private void Forecast(int beforeStep, int userId, Type[] inputTypes)
    {
        var timePointWithLerp = timePointWithLerpDict[userId][inputTypes];
        if (UseForecast && !timePointWithLerp.IsPlaying)
        {
            var tracks = GetUserInputDataByInputTypes(beforeStep, userId, inputTypes);
            var tickId = 0;
            var deltaTime = Fix64.Zero;
            for (int i = 0; i < tracks.Length; i++)
            {
                if (tracks[i] == null) { continue; }
                tickId = tracks[i].TickId;
                deltaTime = tracks[i].DeltaTime;
                break;
            }
            timePointWithLerp.Forecast(deltaTime, new TimePointData(tickId, tracks), MaxForecastSteps);
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        LockstepUtility.OnRestart -= OnRestart;
    }
}
