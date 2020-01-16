using UniEasy.ECS;

public struct ForwardTimelineData
{
    public IEntity Entity;
    public TimePointData TimePointData;

    public ForwardTimelineData(IEntity entity, TimePointData data)
    {
        Entity = entity;
        TimePointData = data;
    }

    public int TickId
    {
        get { return TimePointData.TickId; }
    }

    public float ForecastCount
    {
        get { return TimePointData.ForecastCount; }
    }

    public float DeltaTime
    {
        get { return TimePointData.DeltaTime; }
    }

    public UserInputData[][] UserInputData
    {
        get { return TimePointData.UserInputData; }
    }
}
