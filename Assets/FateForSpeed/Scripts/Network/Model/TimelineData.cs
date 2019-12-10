using UniEasy.ECS;

public class TimelineData
{
    public IEntity Entity;
    public UserInputData[] UserInputData;
    public float DeltaTime;

    public TimelineData(IEntity entity, UserInputData[] userInputData, float deltaTime)
    {
        Entity = entity;
        UserInputData = userInputData;
        DeltaTime = deltaTime;
    }
}
