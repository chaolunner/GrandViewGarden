using System.Collections.Generic;
using UniRx.Triggers;
using UnityEngine;
using UniEasy.ECS;
using UniEasy;
using UniRx;

public class PublishEventByListenerSystem : SystemBehaviour
{
    protected IGroup publishEventByListeners;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        publishEventByListeners = this.Create(typeof(PublishEventByListener), typeof(ViewComponent));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        publishEventByListeners.OnAdd().Subscribe(entity =>
        {
            var publishEventByListener = entity.GetComponent<PublishEventByListener>();
            var viewComponent = entity.GetComponent<ViewComponent>();

            if (entity.HasComponent<TriggerEnterListener>())
            {
                var listener = entity.GetComponent<TriggerEnterListener>();

                listener.Targets.OnTriggerEnterAsObservable().Subscribe(_ =>
                {
                    if (publishEventByListener.Identifier)
                    {
                        PublishEvents(publishEventByListener.Identifier, publishEventByListener.Events);
                    }
                    else
                    {
                        PublishEvents(viewComponent.Transforms[0].gameObject, publishEventByListener.Events);
                    }
                }).AddTo(this.Disposer).AddTo(publishEventByListener.Disposer);
            }
        }).AddTo(this.Disposer);
    }

    private void PublishEvents(Object source, List<InspectableObjectData> events)
    {
        foreach (var evt in events)
        {
            var message = evt.CreateInstance(false);
            var serializableEvent = message as ISerializableEvent;

            serializableEvent.Source = source;
            EventSystem.Publish(message);
        }
    }
}
