using System;

public interface IUserInputResult
{
    void Rollback();
}

public class UserInputResult<T> : IUserInputResult where T : struct
{
    private Action<T> action;
    private T value;

    public UserInputResult(Action<T> action, T value)
    {
        this.action = action;
        this.value = value;
    }

    public void Rollback()
    {
        action?.Invoke(value);
    }
}
