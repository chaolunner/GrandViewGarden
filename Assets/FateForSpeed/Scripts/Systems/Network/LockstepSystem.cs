using UniEasy.ECS;

public class LockstepSystem : SystemBehaviour
{
    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);
    }

    public override void OnEnable()
    {
        base.OnEnable();
    }
}
