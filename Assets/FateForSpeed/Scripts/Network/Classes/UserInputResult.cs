using System;

public interface IUserInputResult
{
    void Execute();
    void Rollback();
}
public class UserInputResult<T> : IUserInputResult where T : struct
{
    private Action<T, float> action;
    private T value;
    private float deltaTime;

    public UserInputResult(Action<T, float> action, T value, float deltaTime)
    {
        this.action = action;
        this.value = value;
        this.deltaTime = deltaTime;
    }

    public void Execute()
    {
        action?.Invoke(value, deltaTime);
    }

    public void Rollback()
    {
        action?.Invoke(value, -deltaTime);
    }
}
