using System.Collections.Generic;
using UnityEngine;
using UniEasy.ECS;
using UniEasy;

public class PublishEventByListener : ComponentBehaviour
{
    public IdentificationObject Identifier;
    [Reorderable, DropdownMenu(typeof(ISerializableEvent)), RuntimeObject]
    public List<string> Events;
    [Reorderable, ComponentReference]
    public List<ComponentReference> References;
}
