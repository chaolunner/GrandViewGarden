using UniEasy;

[ContextMenu("General/LevelSelectedEvent")]
public class LevelSelectedEvent : SerializableEvent
{
    public int Index;

    public LevelSelectedEvent(int index)
    {
        Index = index;
    }
}
