using UnityEngine;
using UniEasy.ECS;
using UniEasy.Net;
using Common;
using UniRx;

public class UserSystem : NetworkSystemBehaviour
{
    [SerializeField]
    private GameObject UserPrefab;
    private IGroup UserComponents;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        UserComponents = this.Create(typeof(UserComponent), typeof(ViewComponent));
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
                userComponent.UserName.Value = evt.Username;
                userComponent.TotalCount.Value = evt.TotalCount;
                userComponent.WinCount.Value = evt.WinCount;
            });
        }).AddTo(this.Disposer);

        NetworkSystem.Receive(RequestCode.QuitRoom).Subscribe(data =>
        {
            foreach (var entity in UserComponents.Entities)
            {
                var userComponent = entity.GetComponent<UserComponent>();
                var viewComponent = entity.GetComponent<ViewComponent>();

                if (userComponent.UserId == int.Parse(data.StringValue))
                {
                    if (userComponent.IsLocalPlayer || userComponent.IsRoomOwner.Value)
                    {
                        ClearOtherPlayers();
                    }
                    else
                    {
                        Destroy(viewComponent.Transforms[0].gameObject);
                    }
                    break;
                }
            }
        }).AddTo(this.Disposer);
    }

    private void ClearOtherPlayers()
    {
        foreach (var entity in UserComponents.Entities)
        {
            var userComponent = entity.GetComponent<UserComponent>();
            var viewComponent = entity.GetComponent<ViewComponent>();

            if (userComponent.IsLocalPlayer)
            {
                userComponent.IsRoomOwner.Value = false;
            }
            else
            {
                Destroy(viewComponent.Transforms[0].gameObject);
            }
        }
    }
}
