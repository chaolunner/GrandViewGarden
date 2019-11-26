using System.Collections.Generic;
using System;
using Common;

public static class LockstepUtility
{
    public static int RealTickNow { get; private set; } = 0;
    public static event Action OnRestart;
    public static event Action<int> OnSyncTimeline;

    private static List<IInput> inputs = new List<IInput>();
    private static List<LockstepInputs> lockstepInputs = new List<LockstepInputs>();
    private static Dictionary<int, Dictionary<int, Dictionary<Type, IInput>>> inputDict = new Dictionary<int, Dictionary<int, Dictionary<Type, IInput>>>();

    public static void AddInput<T>(T input) where T : IInput
    {
        inputs.Add(input);
    }

    public static void AddToTimeline(LockstepInputs inputs)
    {
        if (lockstepInputs.Count > inputs.TickId)
        {
            for (int i = inputs.TickId; i < lockstepInputs.Count; i++)
            {
                inputDict.Remove(i);
            }
            lockstepInputs.RemoveRange(inputs.TickId, lockstepInputs.Count - inputs.TickId);
        }
        if (lockstepInputs.Count == 0)
        {
            RealTickNow = 0;
            OnRestart?.Invoke();
        }
        if (inputs.TickId > lockstepInputs.Count)
        {
            OnSyncTimeline?.Invoke(lockstepInputs.Count);
            return;
        }
        lockstepInputs.Add(inputs);
        inputDict.Add(inputs.TickId, new Dictionary<int, Dictionary<Type, IInput>>());
        if (inputs.UserInputs != null)
        {
            foreach (var userInputs in inputs.UserInputs)
            {
                inputDict[inputs.TickId].Add(userInputs.UserId, new Dictionary<Type, IInput>());
                if (userInputs.InputData != null)
                {
                    foreach (var inputData in userInputs.InputData)
                    {
                        var input = InputUtility.FromInputData(inputData);
                        inputDict[inputs.TickId][userInputs.UserId].Add(input.GetType(), (IInput)input);
                    }
                }
            }
        }
    }

    public static UserInputs GetUserInputs()
    {
        UserInputs userInputs = new UserInputs();
        userInputs.InputData = InputUtility.ToInputData(inputs);
        inputs.Clear();
        RealTickNow++;
        return userInputs;
    }

    public static UserInputData<T> GetUserInputData<T>(int tickId, int userId) where T : IInput
    {
        return GetUserInputData(tickId, userId, typeof(T)) as UserInputData<T>;
    }

    public static UserInputData GetUserInputData(int tickId, int userId, Type inputType)
    {
        if (inputDict.ContainsKey(tickId))
        {
            var data = new UserInputData();
            data.TickId = tickId;
            if (inputDict[tickId].ContainsKey(userId))
            {
                data.UserId = userId;
                data.DeltaTime = lockstepInputs[tickId].DeltaTime;
                if (inputDict[tickId][userId].ContainsKey(inputType))
                {
                    data.Input = inputDict[tickId][userId][inputType];
                }
            }
            return data;
        }
        return null;
    }
}
