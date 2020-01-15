using System.Collections.Generic;
using System;
using Common;

public static class LockstepUtility
{
    public static int Index;
    public static event Action OnRestart;

    private static List<LockstepInputs> lockstepInputs = new List<LockstepInputs>();
    private static Dictionary<Type, List<IInput>> inputDict = new Dictionary<Type, List<IInput>>();
    private static Dictionary<int, List<Dictionary<int, Dictionary<Type, List<IInput>>>>> inputTrackDict = new Dictionary<int, List<Dictionary<int, Dictionary<Type, List<IInput>>>>>();

    public static void AddInput<T>(T input) where T : IInput
    {
        var type = typeof(T);
        if (!inputDict.ContainsKey(type))
        {
            inputDict.Add(type, new List<IInput>());
        }
        inputDict[type].Add(input);
    }

    public static void AddToTimeline(LockstepInputs inputs)
    {
        if (inputs.TickId < 0)
        {
            Index = 0;
            lockstepInputs.Clear();
            OnRestart?.Invoke();
        }
        if (inputs.TickId != lockstepInputs.Count)
        {
            return;
        }
        lockstepInputs.Add(inputs);
        inputTrackDict.Add(inputs.TickId, new List<Dictionary<int, Dictionary<Type, List<IInput>>>>());
        if (inputs.UserInputs != null && inputs.UserInputs.Length > 0)
        {
            for (int i = 0; i < inputs.UserInputs.Length; i++)
            {
                var dict = new Dictionary<int, Dictionary<Type, List<IInput>>>();
                for (int j = 0; j < inputs.UserInputs[i].Length; j++)
                {
                    var userInputs = inputs.UserInputs[i][j];
                    dict.Add(userInputs.UserId, new Dictionary<Type, List<IInput>>());
                    if (userInputs.InputData != null)
                    {
                        for (int k = 0; k < userInputs.InputData.Length; k++)
                        {
                            var input = MessagePackUtility.Deserialize<IInput>(userInputs.InputData[k]);
                            var type = input.GetType();
                            if (!dict[userInputs.UserId].ContainsKey(type))
                            {
                                dict[userInputs.UserId].Add(type, new List<IInput>());
                            }
                            dict[userInputs.UserId][type].Add(input);
                        }
                    }
                }
                inputTrackDict[inputs.TickId].Add(dict);
            }
        }
    }

    public static UserInputs GetUserInputs()
    {
        var bytes = new List<byte[]>();
        var e = inputDict.GetEnumerator();
        while (e.MoveNext())
        {
            for (int i = 0; i < e.Current.Value.Count; i++)
            {
                bytes.Add(MessagePackUtility.Serialize(e.Current.Value[i]));
            }
        }
        UserInputs userInputs = new UserInputs();
        userInputs.Index = Index;
        userInputs.TickId = lockstepInputs.Count;
        userInputs.InputData = bytes.ToArray();
        inputDict.Clear();
        Index++;
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

    public static UserInputData[] GetUserInputData(int tickId, int userId, Type inputType)
    {
        if (HasTickId(tickId))
        {
            var dataList = new List<UserInputData>();
            for (int i = 0; i < inputTrackDict[tickId].Count; i++)
            {
                if (inputTrackDict[tickId][i].ContainsKey(userId))
                {
                    var data = CreateUesrInputData(tickId);
                    var dict = inputTrackDict[tickId][i][userId];
                    data.UserId = userId;
                    if (dict.ContainsKey(inputType))
                    {
                        data.Inputs = dict[inputType].ToArray();
                    }
                    dataList.Add(data);
                }
            }
            if (dataList.Count > 0) { return dataList.ToArray(); }
        }
        return null;
    }

    public static UserInputData[][] GetAllUserInputData(int tickId, Type inputType)
    {
        if (HasTickId(tickId))
        {
            var dataList = new List<List<UserInputData>>();
            for (int i = 0; i < inputTrackDict[tickId].Count; i++)
            {
                dataList.Add(new List<UserInputData>());
                var e = inputTrackDict[tickId][i].GetEnumerator();
                while (e.MoveNext())
                {
                    if (e.Current.Value.ContainsKey(inputType))
                    {
                        var data = CreateUesrInputData(tickId);
                        data.UserId = e.Current.Key;
                        data.Inputs = e.Current.Value[inputType].ToArray();
                        dataList[i].Add(data);
                    }
                }
            }
            if (dataList.Count > 0)
            {
                var result = new UserInputData[dataList.Count][];
                for (int i = 0; i < dataList.Count; i++)
                {
                    result[i] = new UserInputData[dataList[i].Count];
                    dataList[i].CopyTo(result[i]);
                }
                return result;
            }
        }
        return null;
    }

    public static UserInputData CreateUesrInputData(int tickId)
    {
        if (HasTickId(tickId))
        {
            var data = new UserInputData();
            data.TickId = tickId;
            data.DeltaTime = lockstepInputs[tickId].DeltaTime;
            data.Inputs = new IInput[0];
            return data;
        }
        return null;
    }
}
