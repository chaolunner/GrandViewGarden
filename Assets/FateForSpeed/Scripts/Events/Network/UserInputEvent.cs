public class UserInputEvent
{
    public int UserId;
    public int TickId;
    public object Input;

    public UserInputEvent(int userId, int tickId, object input)
    {
        UserId = userId;
        TickId = tickId;
        Input = input;
    }
}
