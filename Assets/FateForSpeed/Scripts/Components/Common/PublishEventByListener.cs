using System.Collections.Generic;
using UnityEngine;
using UniEasy.ECS;
using UniEasy;

public class PublishEventByListener : ComponentBehaviour
{
    public IdentificationObject Identifier;
    [Reorderable(elementName: null), DropdownMenu(typeof(ISerializableEvent))]
    public List<string> Events;
    [Reorderable(elementName: null)]
    public List<GameObject> References;
}
