using UnityEngine;
using UniEasy.ECS;
using Common;
using UniRx;

public class PhysicsSystem : NetworkSystemBehaviour
{
    private NetworkGroup Network;
    private INetworkTimeline NetwrokTimeline;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);
        Network = LockstepFactory.Create();
        NetwrokTimeline = Network.CreateTimeline(typeof(EventInput));
    }

    public override void OnEnable()
    {
        base.OnEnable();
        Physics.autoSimulation = false;
        NetwrokTimeline.OnForward(_ =>
        {
            Physics.Simulate(Time.fixedDeltaTime);
            return null;
        }).AddTo(this.Disposer);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        Physics.autoSimulation = true;
    }
}
