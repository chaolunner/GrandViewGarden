using System.Collections.Generic;
using UniEasy.ECS;
using UnityEngine;
using UniEasy;

[AddComponentMenu("Listeners/ListenerBehaviour")]
public class ListenerBehaviour<T> : ComponentBehaviour, IListener<T>
{
    [SerializeField, Reorderable(elementName: null, isDrawObjectReference: false)]
    protected List<T> targets;

    public virtual List<T> Targets
    {
        get { return targets; }
        set { targets = value; }
    }
}
