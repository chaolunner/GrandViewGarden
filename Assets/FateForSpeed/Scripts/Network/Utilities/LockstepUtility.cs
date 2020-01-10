using System.Collections.Generic;
using System;
using Common;

public static class LockstepUtility
{
    public static int RealTickNow { get; private set; } = 0;
    public static event Action OnRestart;
    public static event Action<int> OnSyncTimeline;

    private static List<LockstepInputs> lockstepInputs = new List<LockstepInputs>();
    private static Dictionary<Type, IInput> inputDict = new Dictionary<Type, IInput>();
    private static Dictionary<int, Dictionary<int, Dictionary<Type, IInput>>> inputTrackDict = new Dictionary<int, Dictionary<int, Dictionary<Type, IInput>>>();

    private readonly static Type EventInputType = typeof(EventInput);

    public static void AddInput<T>(T input) where T : IInput
    {
        if (!inputDict.ContainsKey(typeof(T)))
        {
            inputDict.Add(typeof(T), input);
        }
    }

    public static void AddEventInput(EventCode type, string msg)
    {
        if (!inputDict.ContainsKey(EventInputType))
        {
            inputDict.Add(EventInputType, new EventInput());
        }
        var eventInput = inputDict[EventInputType] as EventInput;
        eventInput.Write(type, msg);
    }

    public static void AddToTimeline(LockstepInputs inputs)
    {
        if (lockstepInputs.Count > inputs.TickId)
        {
            for (int i = inputs.TickId; i < lockstepInputs.Count; i++)
            {
                inputTrackDict.Remove(i);
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
        inputTrackDict.Add(inputs.TickId, new Dictionary<int, Dictionary<Type, IInput>>());
        if (inputs.UserInputs != null)
        {
            foreach (var userInputs in inputs.UserInputs)
            {
                inputTrackDict[inputs.TickId].Add(userInputs.UserId, new Dictionary<Type, IInput>());
                if (userInputs.InputData != null)
                {
                    foreach (var inputData in userInputs.InputData)
                    {
                        var input = MessagePackUtility.Deserialize<IInput>(inputData);
                        inputTrackDict[inputs.TickId][userInputs.UserId].Add(input.GetType(), input);
                    }
                }
            }
        }
    }

    public static UserInputs GetUserInputs()
    {
        int index = 0;
        UserInputs userInputs = new UserInputs();
        userInputs.InputData = new byte[inputDict.Count][];
        foreach (var kvp in inputDict)
        {
            userInputs.InputData[index] = MessagePackUtility.Serialize(kvp.Value);
            index++;
        }
        inputDict.Clear();
        RealTickNow++;
        return userInputs;
    }

    public static bool HasTickId(int tickId)
    {
        return inputTrackDict.ContainsKey(tickId);
    }

    public static Fix64 GetDeltaTime(int tickId)
    {
        if (HasTickId(tickId))
        {
            return lockstepInputs[tickId].DeltaTime;
        }
        return Fix64.Zero;
    }

    public static UserInputData GetUserInputData(int tickId, int userId, Type inputType)
    {
        if (HasTickId(tickId))
        {
            var data = new UserInputData();
            data.TickId = tickId;
            data.DeltaTime = lockstepInputs[tickId].DeltaTime;
            if (inputTrackDict[tickId].ContainsKey(userId))
            {
                data.UserId = userId;
                if (inputTrackDict[tickId][userId].ContainsKey(inputType))
                {
                    data.Input = inputTrackDict[tickId][userId][inputType];
                }
            }
            return data;
        }
        return null;
    }

    public static UserInputData[] GetAllUserInputData(int tickId)
    {
        if (HasTickId(tickId))
        {
            var userInputData = new List<UserInputData>();
            var e1 = inputTrackDict[tickId].GetEnumerator();
            while (e1.MoveNext())
            {
                var e2 = e1.Current.Value.GetEnumerator();
                while (e2.MoveNext())
                {
                    var data = new UserInputData();
                    data.TickId = tickId;
                    data.DeltaTime = lockstepInputs[tickId].DeltaTime;
                    data.UserId = e1.Current.Key;
                    data.Input = e2.Current.Value;
                    userInputData.Add(data);
                }
            }
            return userInputData.ToArray();
        }
        return null;
    }
}
