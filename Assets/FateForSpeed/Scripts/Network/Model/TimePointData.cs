public class TimePointData
{
    public int TickId;
    public int ForecastCount;
    public float Start;
    public float End;
    public float DeltaTime;
    public UserInputData[][] UserInputData;

    public TimePointData(int tickId, UserInputData[] userInputData)
    {
        TickId = tickId;
        UserInputData = new UserInputData[][] { userInputData };
    }

    public TimePointData(int tickId, UserInputData[][] userInputData)
    {
        TickId = tickId;
        UserInputData = userInputData;
    }
}
