using UniEasy.ECS;

public enum LevelSelectionMode
{
    Single,
    Additive,
    Smart,
}

public class LevelSelecter : ComponentBehaviour
{
    public LevelSelectionMode Mode;
    public int Index;
}
