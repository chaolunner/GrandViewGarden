using UnityEngine;
using UniEasy.ECS;
using UniRx;

public class BulletSystem : NetworkSystemBehaviour
{
    private IGroup BulletComponents;
    private NetworkGroup Network;
    private INetworkTimeline NetwrokTimeline;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        BulletComponents = this.Create(typeof(BulletComponent), typeof(NetworkIdentityComponent), typeof(ViewComponent));
        Network = LockstepFactory.Create(BulletComponents);
        NetwrokTimeline = Network.CreateTimeline();
    }

    public override void OnEnable()
    {
        base.OnEnable();

        NetwrokTimeline.OnReverse((entity, result) =>
        {
        }).AddTo(this.Disposer);

        NetwrokTimeline.OnForward((entity, userInputData, deltaTime, tickId) =>
        {
            var viewComponent = entity.GetComponent<ViewComponent>();

            viewComponent.Transforms[0].Translate(new Vector3(0, 0, 0.01f), Space.Self);

            return null;
        }).AddTo(this.Disposer);
    }
}
