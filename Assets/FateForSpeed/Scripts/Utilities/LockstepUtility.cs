using System.Collections.Generic;
using Common;

public static class LockstepUtility
{
    private static List<IInput> inputs = new List<IInput>();
    private static List<LockstepInputs> lockstepInputs = new List<LockstepInputs>();

    public static void AddInput<T>(T input) where T : IInput
    {
        inputs.Add(input);
    }

    public static void AddToHistory(LockstepInputs inputs)
    {
        if (lockstepInputs.Count > inputs.TickId)
        {
            lockstepInputs.RemoveRange(inputs.TickId, lockstepInputs.Count - inputs.TickId);
        }
        lockstepInputs.Add(inputs);
    }

    public static UserInputs GetUserInputs()
    {
        UserInputs userInputs = new UserInputs();
        userInputs.InputData = InputUtility.ToInputData(inputs);
        inputs.Clear();
        return userInputs;
    }
}
