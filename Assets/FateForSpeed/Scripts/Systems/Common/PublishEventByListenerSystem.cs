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

            entity.OnListenerAsObservable().Subscribe(_ =>
            {
                if (publishEventByListener.Identifier)
                {
                    PublishEvents(publishEventByListener.Identifier, publishEventByListener.Events, publishEventByListener.References);
                }
                else
                {
                    PublishEvents(viewComponent.Transforms[0].gameObject, publishEventByListener.Events, publishEventByListener.References);
                }
            }).AddTo(this.Disposer).AddTo(publishEventByListener.Disposer);
        }).AddTo(this.Disposer);
    }

    private void PublishEvents(Object source, List<string> events, List<ComponentReference> references = null)
    {
        foreach (var evt in events)
        {
            var message = RuntimeObject.FromJson(evt);
            var serializableEvent = message as ISerializableEvent;

            serializableEvent.Source = source;
            serializableEvent.References = references;
            EventSystem.Publish(message);
        }
    }
}
