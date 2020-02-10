using System.Collections.Generic;
using UniEasy.ECS;
using UnityEngine;
using UniEasy;

[AddComponentMenu("Listeners/ListenerBehaviour")]
public class ListenerBehaviour<T1, T2> : ComponentBehaviour, IListener<T1>
{
    [SerializeField, Reorderable, TypePopup(typeof(ISerializableEvent))]
    protected List<T1> targets;
    [SerializeField, Reorderable]
    protected List<T2> references;

    public virtual List<T1> Targets
    {
        get { return targets; }
        set { targets = value; }
    }

    public virtual List<T2> References
    {
        get { return references; }
        set { references = value; }
    }
}
