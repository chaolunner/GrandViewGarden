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
        NetworkPlayerComponents = this.Create(typeof(NetworkPlayerComponent), typeof(ViewComponent));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        NetworkPlayerComponents.OnAdd().Subscribe(entity1 =>
        {
            var networkPlayerComponent = entity1.GetComponent<NetworkPlayerComponent>();
            var viewComponent = entity1.GetComponent<ViewComponent>();

            if (networkPlayerComponent.IsLocalPlayer)
            {
                foreach (var entity2 in FollowCameraComponents.Entities)
                {
                    var followCameraComponent = entity2.GetComponent<FollowCameraComponent>();
                    var virtualCamera = entity2.GetComponent<CinemachineVirtualCamera>();

                    virtualCamera.Follow = viewComponent.Transforms[0];
                }
            }
        }).AddTo(this.Disposer);
    }
}
