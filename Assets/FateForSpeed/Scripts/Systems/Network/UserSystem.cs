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
            PrefabFactory.Instantiate(UserPrefab, null, false, go =>
            {
                var userComponent = go.GetComponent<UserComponent>();

                userComponent.IsLocalPlayer = evt.IsLocalPlayer;
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
