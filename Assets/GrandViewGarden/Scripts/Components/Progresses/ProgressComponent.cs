using UniEasy.ECS;
using UniRx;

public class ProgressComponent : ComponentBehaviour
{
    [RangeReactiveProperty(0, 1)]
    public FloatReactiveProperty Progress;
}
