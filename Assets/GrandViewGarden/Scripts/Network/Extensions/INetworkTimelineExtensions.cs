using UniEasy.ECS;
using System;
using Common;

public static class INetworkTimelineExtensions
{
    public static IDisposable OnForward(this INetworkTimeline timeline, Func<IEntity, UserInputData[], Fix64, IUserInputResult[]> onNext)
    {
        Func<ForwardTimelineData, IUserInputResult[]> func = data =>
        {
            return onNext(data.Entity, data.UserInputData[0], data.DeltaTime);
        };
        return timeline.OnForward(func);
    }

    public static IDisposable OnForward(this INetworkTimeline timeline, Func<UserInputData[][], Fix64, IUserInputResult[]> onNext)
    {
        Func<ForwardTimelineData, IUserInputResult[]> func = data =>
        {
            return onNext(data.UserInputData, data.DeltaTime);
        };
        return timeline.OnForward(func);
    }

    public static IDisposable OnReverse(this INetworkTimeline timeline, Action<IEntity, IUserInputResult[]> onNext)
    {
        Action<ReverseTimelineData> action = data =>
        {
            onNext(data.Entity, data.UserInputResult);
        };
        return timeline.OnReverse(action);
    }

    public static IDisposable OnReverse(this INetworkTimeline timeline, Action<IUserInputResult[]> onNext)
    {
        Action<ReverseTimelineData> action = data =>
        {
            onNext(data.UserInputResult);
        };
        return timeline.OnReverse(action);
    }
}
