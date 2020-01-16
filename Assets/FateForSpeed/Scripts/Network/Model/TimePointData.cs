using Common;

public class TimePointData
{
    public bool Physics;
    public int TickId;
    public int ForecastCount;
    public Fix64 Duration;
    public float DeltaTime;
    public UserInputData[][] UserInputData;

    public TimePointData(int tickId, Fix64 duration, UserInputData[] userInputData)
    {
        TickId = tickId;
        Duration = duration;
        UserInputData = new UserInputData[][] { userInputData };
    }

    public TimePointData(int tickId, Fix64 duration, UserInputData[][] userInputData)
    {
        TickId = tickId;
        Duration = duration;
        UserInputData = userInputData;
    }
}
