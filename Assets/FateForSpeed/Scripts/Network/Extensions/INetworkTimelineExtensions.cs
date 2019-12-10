using UniEasy.ECS;
using System;

public static class INetworkTimelineExtensions
{
    public static IDisposable Subscribe(this INetworkTimeline subject, Func<TimelineData, IUserInputResult[]> onNext)
    {
        return subject.Subscribe(new NetworkTimeline(onNext));
    }

    public static IDisposable Subscribe(this INetworkTimeline subject, Func<IEntity, UserInputData[], float, IUserInputResult[]> onNext)
    {
        Func<TimelineData, IUserInputResult[]> func = data =>
        {
            return onNext(data.Entity, data.UserInputData, data.DeltaTime);
        };
        return subject.Subscribe(new NetworkTimeline(func));
    }
}
