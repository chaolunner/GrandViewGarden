using UniEasy.ECS;

public struct ReverseTimelineData
{
    public IEntity Entity;
    public IUserInputResult[] UserInputResult;

    public ReverseTimelineData(IEntity entity, IUserInputResult[] userInputResult)
    {
        Entity = entity;
        UserInputResult = userInputResult;
    }
}
