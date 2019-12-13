using UniEasy.ECS;

public struct ForwardTimelineData
{
    public IEntity Entity;
    public UserInputData[] UserInputData;
    public float DeltaTime;

    public ForwardTimelineData(IEntity entity, UserInputData[] userInputData, float deltaTime)
    {
        Entity = entity;
        UserInputData = userInputData;
        DeltaTime = deltaTime;
    }
}
