using UniEasy;

[ContextMenu("Common/LevelSelectedEvent")]
public class LevelSelectedEvent : SerializableEvent
{
    public int Index;

    public LevelSelectedEvent()
    {
    }

    public LevelSelectedEvent(int index)
    {
        Index = index;
    }
}
