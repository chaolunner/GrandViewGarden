using Common;

public class UserInputData<T> where T : IInput
{
    public int TickId;
    public int UserId = -1;
    public Fix64 DeltaTime;
    public T Input;
}

public class UserInputData : UserInputData<IInput> { }
