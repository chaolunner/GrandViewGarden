using UniEasy;
using System;

public interface INetworkTimeline : IFuncSubject<TimelineData, IUserInputResult[]> { }

public class NetworkTimeline : FuncSubject<TimelineData, IUserInputResult[]>, INetworkTimeline
{
    public NetworkTimeline(Func<TimelineData, IUserInputResult[]> onNext = null, Action onCompleted = null, Action<Exception> onError = null) : base(onNext, onCompleted, onError) { }
}
