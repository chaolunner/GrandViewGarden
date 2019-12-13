using System.Collections.Generic;
using UniEasy;
using System;

public interface INetworkTimeline
{
    List<IUserInputResult[]> Forward(ForwardTimelineData data);
    void Reverse(ReverseTimelineData data);
    IDisposable OnForward(Func<ForwardTimelineData, IUserInputResult[]> onNext);
    IDisposable OnReverse(Action<ReverseTimelineData> onNext);
}

public class NetworkTimeline : INetworkTimeline
{
    private IFuncSubject<ForwardTimelineData, IUserInputResult[]> forwardSubject;
    private IActionSubject<ReverseTimelineData> reverseSubject;

    public NetworkTimeline()
    {
        forwardSubject = new FuncSubject<ForwardTimelineData, IUserInputResult[]>();
        reverseSubject = new ActionSubject<ReverseTimelineData>();
    }

    public IDisposable OnForward(Func<ForwardTimelineData, IUserInputResult[]> onNext)
    {
        var subSubject = new FuncSubject<ForwardTimelineData, IUserInputResult[]>(onNext);
        forwardSubject.Subscribe(subSubject);
        return subSubject;
    }

    public IDisposable OnReverse(Action<ReverseTimelineData> onNext)
    {
        var subSubject = new ActionSubject<ReverseTimelineData>(onNext);
        reverseSubject.Subscribe(onNext);
        return subSubject;
    }

    public List<IUserInputResult[]> Forward(ForwardTimelineData data)
    {
        return forwardSubject.OnNext(data);
    }

    public void Reverse(ReverseTimelineData data)
    {
        reverseSubject.OnNext(data);
    }
}
