using UniRx.Triggers;
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
        BulletComponents = this.Create(typeof(BulletComponent), typeof(NetworkIdentityComponent), typeof(ViewComponent), typeof(Rigidbody));
        Network = LockstepFactory.Create(BulletComponents);
        NetwrokTimeline = Network.CreateTimeline(typeof(EventInput));
    }

    public override void OnEnable()
    {
        base.OnEnable();
        BulletComponents.OnAdd().Subscribe(entity =>
        {
            var bulletComponent = entity.GetComponent<BulletComponent>();
            var rigidbody = entity.GetComponent<Rigidbody>();

            bulletComponent.OnEnableAsObservable().Subscribe(_ =>
            {
                rigidbody.WakeUp();
            }).AddTo(this.Disposer).AddTo(bulletComponent.Disposer);

            bulletComponent.OnDisableAsObservable().Subscribe(_ =>
            {
                bulletComponent.Collision = null;
                rigidbody.isKinematic = false;
                rigidbody.Sleep();
            }).AddTo(this.Disposer).AddTo(bulletComponent.Disposer);

            bulletComponent.OnCollisionEnterAsObservable().Subscribe(col =>
            {
                bulletComponent.Collision = col;
                rigidbody.isKinematic = true;
            }).AddTo(this.Disposer).AddTo(bulletComponent.Disposer);
        }).AddTo(this.Disposer);

        NetwrokTimeline.OnForward(data =>
        {
            var bulletComponent = data.Entity.GetComponent<BulletComponent>();
            var viewComponent = data.Entity.GetComponent<ViewComponent>();
            var rigidbody = data.Entity.GetComponent<Rigidbody>();

            if (bulletComponent.Collision != null)
            {
                //if (data.Start == 0)
                //{
                //    PoolFactory.Push(data.Entity);
                //}
            }
            else
            {
                var position = (FixVector3)viewComponent.Transforms[0].position + bulletComponent.Velocity * data.DeltaTime;
                rigidbody.MovePosition((Vector3)position);
            }
            return null;
        }).AddTo(this.Disposer);
    }
}
