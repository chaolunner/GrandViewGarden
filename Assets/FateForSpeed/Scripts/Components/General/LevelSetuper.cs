using System.Collections.Generic;
using UniEasy.ECS;
using UniEasy;
using UniRx;

public class LevelSetuper : ComponentBehaviour
{
    public IntReactiveProperty Index;
    [Reorderable]
    public List<FSMToggle> Levels;
}
