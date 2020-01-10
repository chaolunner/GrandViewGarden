using UniEasy.ECS;

public struct ForwardTimelineData
{
    public IEntity Entity;
    public TimePointData TimePointData;
    public float DeltaTime;

    public ForwardTimelineData(IEntity entity, TimePointData timePointData, float deltaTime)
    {
        Entity = entity;
        TimePointData = timePointData;
        DeltaTime = deltaTime;
    }
}
