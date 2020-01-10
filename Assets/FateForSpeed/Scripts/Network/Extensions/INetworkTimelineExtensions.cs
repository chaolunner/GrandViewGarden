using UniEasy.ECS;
using System;

public static class INetworkTimelineExtensions
{
    public static IDisposable OnForward(this INetworkTimeline timeline, Func<IEntity, UserInputData[], float, IUserInputResult[]> onNext)
    {
        Func<ForwardTimelineData, IUserInputResult[]> func = data =>
        {
            return onNext(data.Entity, data.TimePointData.Tracks[0], data.DeltaTime);
        };
        return timeline.OnForward(func);
    }

    public static IDisposable OnForward(this INetworkTimeline timeline, Func<UserInputData[][], float, IUserInputResult[]> onNext)
    {
        Func<ForwardTimelineData, IUserInputResult[]> func = data =>
        {
            return onNext(data.TimePointData.Tracks, data.DeltaTime);
        };
        return timeline.OnForward(func);
    }

    public static IDisposable OnForward(this INetworkTimeline timeline, Func<IEntity, UserInputData[], float, int, IUserInputResult[]> onNext)
    {
        Func<ForwardTimelineData, IUserInputResult[]> func = data =>
        {
            return onNext(data.Entity, data.TimePointData.Tracks[0], data.DeltaTime, data.TimePointData.TickId);
        };
        return timeline.OnForward(func);
    }

    public static IDisposable OnForward(this INetworkTimeline timeline, Func<UserInputData[][], float, int, IUserInputResult[]> onNext)
    {
        Func<ForwardTimelineData, IUserInputResult[]> func = data =>
        {
            return onNext(data.TimePointData.Tracks, data.DeltaTime, data.TimePointData.TickId);
        };
        return timeline.OnForward(func);
    }

    public static IDisposable OnForward(this INetworkTimeline timeline, Func<IEntity, UserInputData[], float, int, int, IUserInputResult[]> onNext)
    {
        Func<ForwardTimelineData, IUserInputResult[]> func = data =>
        {
            return onNext(data.Entity, data.TimePointData.Tracks[0], data.DeltaTime, data.TimePointData.TickId, data.TimePointData.ForecastCount);
        };
        return timeline.OnForward(func);
    }

    public static IDisposable OnForward(this INetworkTimeline timeline, Func<UserInputData[][], float, int, int, IUserInputResult[]> onNext)
    {
        Func<ForwardTimelineData, IUserInputResult[]> func = data =>
        {
            return onNext(data.TimePointData.Tracks, data.DeltaTime, data.TimePointData.TickId, data.TimePointData.ForecastCount);
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
