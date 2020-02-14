using UnityEngine;
using UniEasy.ECS;
using UniEasy.DI;
using Common;
using UniRx;

public class BulletSystem : NetworkSystemBehaviour
{
    [Inject]
    private IPoolFactory PoolFactory;
    private IGroup BulletComponents;
    private NetworkGroup Network;
    private INetworkTimeline NetwrokTimeline;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);
        BulletComponents = this.Create(typeof(BulletComponent), typeof(NetworkIdentityComponent), typeof(ViewComponent), typeof(CapsuleCollider));
        Network = LockstepFactory.Create(BulletComponents, usePhysics: true);
        NetwrokTimeline = Network.CreateTimeline(typeof(EventInput));
    }

    public override void OnEnable()
    {
        base.OnEnable();
        BulletComponents.OnAdd().Subscribe(entity =>
        {
            var bulletComponent = entity.GetComponent<BulletComponent>();
            var viewComponent = entity.GetComponent<ViewComponent>();
            var capsuleCollider = entity.GetComponent<CapsuleCollider>();

            bulletComponent.Radius = 0.5f * Mathf.Max(2 * capsuleCollider.radius, capsuleCollider.height);
        }).AddTo(this.Disposer);

        NetwrokTimeline.OnForward(data =>
        {
            var bulletComponent = data.Entity.GetComponent<BulletComponent>();
            var viewComponent = data.Entity.GetComponent<ViewComponent>();
            var capsuleCollider = data.Entity.GetComponent<CapsuleCollider>();

            if (bulletComponent.Velocity == FixVector3.zero) { return null; }

            var direction = (FixVector3)viewComponent.Transforms[0].forward;
            var offset = (FixVector3)capsuleCollider.center + bulletComponent.Radius * direction;
            var origin = (FixVector3)viewComponent.Transforms[0].position + offset;
            var maxDistance = bulletComponent.Velocity.magnitude * data.DeltaTime;

            RaycastHit hit;

            if (Physics.Raycast((Vector3)origin, (Vector3)direction, out hit, (float)maxDistance))
            {
                viewComponent.Transforms[0].position = (Vector3)(origin + hit.distance * direction - offset);
                bulletComponent.Velocity = FixVector3.zero;
            }
            else
            {
                viewComponent.Transforms[0].position = (Vector3)(origin + maxDistance * direction - offset);
            }
            return null;
        }).AddTo(this.Disposer);
    }
}
