using UnityEngine;
using UniEasy.ECS;
using System.Linq;
using UniEasy.DI;
using UniEasy;
using UniRx;

[System.Serializable]
public struct SerializableEventObject
{
    public string EventType;
    public Object Target;
}

[AddComponentMenu("Listeners/SerializableEventListener")]
public class SerializableEventListener : ListenerBehaviour<SerializableEventObject, GameObject>
{
    protected IEventSystem EventSystem
    {
        get
        {
            if (eventSystem == null)
            {
                eventSystem = ProjectContext.ProjectContainer.Resolve<IEventSystem>();
            }
            return eventSystem;
        }
    }

    private IEventSystem eventSystem;

    public System.IObservable<T> OnEvent<T>(bool onlyOnMainThread = false) where T : ISerializableEvent
    {
        var result = EventSystem.OnEvent<T>().Where(evt => IsValid(evt));
        if (onlyOnMainThread)
        {
            result = result.ObserveOnMainThread();
        }
        return result;
    }

    public bool IsValid<T>(T evt) where T : ISerializableEvent
    {
        return Targets.Any(obj => evt.GetType() == obj.EventType.GetTypeFromCached() && obj.Target == (evt as ISerializableEvent).Source);
    }
}
