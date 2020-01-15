using Common;

public class TimePointData
{
    public int TickId;
    public int ForecastCount;
    public Fix64 RealTime;
    public float Start;
    public float End;
    public float DeltaTime;
    public UserInputData[][] UserInputData;

    public TimePointData(int tickId, Fix64 realTime, UserInputData[] userInputData)
    {
        TickId = tickId;
        RealTime = realTime;
        UserInputData = new UserInputData[][] { userInputData };
    }

    public TimePointData(int tickId, Fix64 realTime, UserInputData[][] userInputData)
    {
        TickId = tickId;
        RealTime = realTime;
        UserInputData = userInputData;
    }
}
