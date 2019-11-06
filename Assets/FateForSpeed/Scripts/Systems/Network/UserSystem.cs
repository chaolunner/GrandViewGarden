using UnityEngine;
using UniEasy.ECS;
using UniRx;

public class UserSystem : SystemBehaviour
{
    [SerializeField]
    private GameObject UserPrefab;
    private IGroup UserComponents;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        UserComponents = this.Create(typeof(UserComponent));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        EventSystem.OnEvent<SpawnUserEvent>(true).Subscribe(evt =>
        {
            foreach (var entity in UserComponents.Entities)
            {
                var userComponent = entity.GetComponent<UserComponent>();
                if (userComponent.UserId == evt.UserId)
                {
                    userComponent.IsRoomOwner.Value = evt.IsRoomOwner;
                    return;
                }
            }
            PrefabFactory.Instantiate(UserPrefab, null, false, go =>
            {
                var userComponent = go.GetComponent<UserComponent>();

                userComponent.IsLocalPlayer = evt.IsLocalPlayer;
                userComponent.IsRoomOwner.Value = evt.IsRoomOwner;
                userComponent.UserId = evt.UserId;
                userComponent.Username.Value = evt.Username;
                userComponent.TotalCount.Value = evt.TotalCount;
                userComponent.WinCount.Value = evt.WinCount;
            });
        }).AddTo(this.Disposer);

        UserComponents.OnAdd().Subscribe(entity =>
        {
            var userComponent = entity.GetComponent<UserComponent>();
        }).AddTo(this.Disposer);
    }
}
