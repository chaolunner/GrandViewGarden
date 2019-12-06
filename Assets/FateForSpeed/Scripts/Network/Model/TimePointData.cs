public class TimePointData
{
    public int TickId;
    public int ForecastCount;
    public UserInputData[] Tracks;

    public TimePointData(int tickId, UserInputData[] tracks)
    {
        TickId = tickId;
        Tracks = tracks;
    }
}
