using Common;

public class UserInputData<T> where T : IInput
{
    public int TickId;
    public int UserId = -1;
    public Fix64 DeltaTime;
    public T[] Inputs;
}

public class UserInputData : UserInputData<IInput>
{
    public T[] GetInputs<T>() where T : IInput
    {
        if (Inputs != null && Inputs.Length > 0)
        {
            T[] inputs = new T[Inputs.Length];
            Inputs.CopyTo(inputs, 0);
            return inputs;
        }
        return null;
    }

    public T GetInput<T>(int index = 0) where T : IInput
    {
        if (Inputs != null && index >= 0 && index < Inputs.Length)
        {
            return (T)Inputs[index];
        }
        return default;
    }

    public T GetLastInput<T>() where T : IInput
    {
        return GetInput<T>(Inputs.Length - 1);
    }
}
