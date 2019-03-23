using System.Collections.Generic;
using UniEasy.ECS;
using UniEasy;

public class PublishEventByListener : ComponentBehaviour
{
    public IdentificationObject Identifier;
    [Reorderable(elementName: null), DropdownMenu(typeof(ISerializableEvent))]
    public List<InspectableObjectData> Events;
}
