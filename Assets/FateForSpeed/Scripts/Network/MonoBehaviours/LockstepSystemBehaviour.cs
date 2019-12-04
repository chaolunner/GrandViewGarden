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
    private int forecastCount = 0;
    private IGroup NetworkPlayerComponents;
    private Dictionary<int, Dictionary<Type[], UserInputWithLerpData>> userInputWithLerpDict = new Dictionary<int, Dictionary<Type[], UserInputWithLerpData>>();

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

                UpdateTimeline(entity);
            }).AddTo(this.Disposer).AddTo(networkPlayerComponent.Disposer);
        }).AddTo(this.Disposer);
    }

    public virtual IInput[] UpdateInputs() { return null; }

    public virtual void UpdateTimeline(IEntity entity) { }

    public void LerpTimeline(IEntity entity, Type[] inputTypes)
    {
        var networkPlayerComponent = entity.GetComponent<NetworkPlayerComponent>();
        var userId = networkPlayerComponent.UserId;
        if (!userInputWithLerpDict.ContainsKey(userId))
        {
            userInputWithLerpDict.Add(userId, new Dictionary<Type[], UserInputWithLerpData>());
        }
        if (!userInputWithLerpDict[userId].ContainsKey(inputTypes))
        {
            userInputWithLerpDict[userId].Add(inputTypes, new UserInputWithLerpData());
        }
        var tickId = userInputWithLerpDict[userId][inputTypes].TickId;
        userInputWithLerpDict[userId][inputTypes].From = userInputWithLerpDict[userId][inputTypes].To;
        userInputWithLerpDict[userId][inputTypes].To = Mathf.Clamp01((userInputWithLerpDict[userId][inputTypes].To * FixedDeltaTime + Time.deltaTime) / FixedDeltaTime);
        PushUntilLastStep(entity, inputTypes);
        ApplyUserInputs(entity, inputTypes);
        userInputWithLerpDict[userId][inputTypes].Loop();
    }

    private int PushUntilLastStep(IEntity entity, Type[] inputTypes)
    {
        var networkPlayerComponent = entity.GetComponent<NetworkPlayerComponent>();
        var userId = networkPlayerComponent.UserId;
        var tickId = userInputWithLerpDict[userId][inputTypes].TickId;
        if (userInputWithLerpDict[userId][inputTypes].UserInputData == null)
        {
            var index = 0;
            userInputWithLerpDict[userId][inputTypes].UserInputData = new List<UserInputData[]>();
            while (index < inputTypes.Length)
            {
                var userInputData = LockstepUtility.GetUserInputData(tickId, userId, inputTypes[index]);
                if (userInputData != null)
                {
                    userInputWithLerpDict[userId][inputTypes].TotalTime += userInputData.DeltaTime;
                    userInputWithLerpDict[userId][inputTypes].UserInputData.Add(GetUserInputDataByInputTypes(tickId, userId, inputTypes));
                    index = 0;
                    tickId++;
                }
                else
                {
                    index++;
                }
            }
        }
        userInputWithLerpDict[userId][inputTypes].TickId = tickId;
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

    private void ApplyUserInputs(IEntity entity, Type[] inputTypes)
    {
        var networkPlayerComponent = entity.GetComponent<NetworkPlayerComponent>();
        var userId = networkPlayerComponent.UserId;
        var time = 0f;
        var from = userInputWithLerpDict[userId][inputTypes].From * userInputWithLerpDict[userId][inputTypes].TotalTime;
        var to = userInputWithLerpDict[userId][inputTypes].To * userInputWithLerpDict[userId][inputTypes].TotalTime;
        if (userInputWithLerpDict[userId][inputTypes].UserInputData != null)
        {
            foreach (var userInputData in userInputWithLerpDict[userId][inputTypes].UserInputData)
            {
                foreach (var data in userInputData)
                {
                    if (data == null) { continue; }
                    var t = time + (float)data.DeltaTime;
                    if (time < (float)from && t > (float)to)
                    {
                        ApplyUserInput(entity, userInputData, (float)(to - from));
                    }
                    else if (time >= (float)from && time <= (float)to && t >= (float)from && t <= (float)to)
                    {
                        ApplyUserInput(entity, userInputData, (float)data.DeltaTime);
                    }
                    else if (time >= (float)from && time <= (float)to)
                    {
                        ApplyUserInput(entity, userInputData, (float)to - time);
                    }
                    else if (t >= (float)from && t <= (float)to)
                    {
                        ApplyUserInput(entity, userInputData, t - (float)from);
                    }
                    time = t;
                    break;
                }
            }
        }
    }

    public virtual void ApplyUserInput(IEntity entity, UserInputData[] userInputData, float deltaTime) { }

    public virtual void OnRestart()
    {
        userInputWithLerpDict.Clear();
        forecastCount = 0;
    }

    //public virtual void PushTimelineWithForecast(IEntity entity, params Type[] inputTypes)
    //{
    //    if (!UseForecast)
    //    {
    //        PushUntilLastStep(entity, inputTypes);
    //        return;
    //    }

    //    var networkPlayerComponent = entity.GetComponent<NetworkPlayerComponent>();
    //    var userId = networkPlayerComponent.UserId;
    //    var beforeStep = tickIdDict.ContainsKey(userId) ? tickIdDict[userId] : 0;
    //    var userInputData = GetUserInputDataByInputTypes(beforeStep, userId, inputTypes);

    //    int count = forecastCount;
    //    while (count > 0)
    //    {
    //        Rollback(count - 1, entity, userInputData);
    //        count--;
    //    }

    //    int afterStep = PushUntilLastStep(entity, inputTypes);
    //    if (afterStep > beforeStep)
    //    {
    //        forecastCount += (beforeStep - afterStep);
    //    }
    //    forecastCount = Mathf.Clamp(forecastCount + 1, 0, MaxForecastSteps);

    //    for (int i = 0; i < forecastCount; i++)
    //    {
    //        Forecast(forecastCount - 1, entity, userInputData);
    //    }

    //    Debug.Log(string.Format(@"Pushed Steps: [{0}] Forecast Count: [{1}]", (afterStep - beforeStep), forecastCount));
    //}

    //public virtual void Forecast(int index, IEntity entity, UserInputData[] userInputData) { }

    //public virtual void Rollback(int index, IEntity entity, UserInputData[] userInputData) { }

    public override void OnDisable()
    {
        base.OnDisable();
        LockstepUtility.OnRestart -= OnRestart;
    }
}
