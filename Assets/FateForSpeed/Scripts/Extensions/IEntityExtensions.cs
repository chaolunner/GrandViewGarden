using UniRx.Triggers;
using UniEasy.ECS;
using System.Linq;
using System;
using UniRx;

public static class IEntityExtensions
{
    public static IObservable<ListenerData> OnListenerAsObservable(this IEntity entity)
    {
        var result = Observable.Empty<ListenerData>();
        if (entity.HasComponent<ClickListener>())
        {
            result = result.Merge(entity.GetComponent<ClickListener>().Targets.OnPointerClickAsObservable()
                .Select(_ => new ListenerData(entity.GetComponent<ClickListener>())));
        }
        if (entity.HasComponent<TriggerEnterListener>())
        {
            result = result.Merge(entity.GetComponent<TriggerEnterListener>().Targets.OnTriggerEnterAsObservable()
                .Select(col => new ListenerData(entity.GetComponent<TriggerEnterListener>(), col)));
        }
        if (entity.HasComponent<TriggerStayListener>())
        {
            result = result.Merge(entity.GetComponent<TriggerStayListener>().Targets.OnTriggerStayAsObservable()
                .Select(col => new ListenerData(entity.GetComponent<TriggerStayListener>(), col)));
        }
        if (entity.HasComponent<TriggerExitListener>())
        {
            result = result.Merge(entity.GetComponent<TriggerExitListener>().Targets.OnTriggerExitAsObservable()
                .Select(col => new ListenerData(entity.GetComponent<TriggerExitListener>(), col)));
        }
        return result;
    }
}
