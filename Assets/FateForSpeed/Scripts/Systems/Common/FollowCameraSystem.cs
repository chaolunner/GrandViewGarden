using UniEasy.ECS;
using Cinemachine;
using UniRx;

public class FollowCameraSystem : SystemBehaviour
{
    private IGroup FollowCameraComponents;
    private IGroup NetworkPlayerComponents;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        FollowCameraComponents = this.Create(typeof(FollowCameraComponent), typeof(CinemachineVirtualCamera));
        NetworkPlayerComponents = this.Create(typeof(NetworkIdentityComponent), typeof(NetworkPlayerComponent), typeof(PlayerControlComponent), typeof(ViewComponent));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        NetworkPlayerComponents.OnAdd().Subscribe(entity1 =>
        {
            var networkIdentityComponent = entity1.GetComponent<NetworkIdentityComponent>();
            var networkPlayerComponent = entity1.GetComponent<NetworkPlayerComponent>();
            var playerControlComponent = entity1.GetComponent<PlayerControlComponent>();
            var viewComponent = entity1.GetComponent<ViewComponent>();

            if (networkIdentityComponent.IsLocalPlayer)
            {
                foreach (var entity2 in FollowCameraComponents.Entities)
                {
                    var followCameraComponent = entity2.GetComponent<FollowCameraComponent>();
                    var virtualCamera = entity2.GetComponent<CinemachineVirtualCamera>();

                    virtualCamera.Follow = viewComponent.Transforms[0];
                    virtualCamera.LookAt = playerControlComponent.LookAt;
                }
            }
        }).AddTo(this.Disposer);
    }
}
