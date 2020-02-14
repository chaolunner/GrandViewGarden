using UnityEngine;
using UniEasy.ECS;
using Common;
using UniRx;

public class UserSystem : NetworkSystemBehaviour
{
    [SerializeField]
    private bool offline;
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
            CreateUser(evt.IsLocalPlayer, evt.IsRoomOwner, evt.UserId, evt.Username, evt.TotalCount, evt.WinCount);
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

        if (offline)
        {
            CreateUser(true, true, 0, "Offline Player", 0, 0);
            NetworkSystem.Mode = SessionMode.Offline;
            LockstepUtility.AddInput(new EventInput(EventCode.GameStart, "True"));
        }
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

    private GameObject CreateUser(bool isLocalPlayer, bool isRoomOwner, int userId, string userName, int totalCount, int winCount)
    {
        return PrefabFactory.Instantiate(UserPrefab, null, false, go =>
        {
            var userComponent = go.GetComponent<UserComponent>();

            userComponent.IsLocalPlayer = isLocalPlayer;
            userComponent.IsRoomOwner.Value = isRoomOwner;
            userComponent.UserId = userId;
            userComponent.UserName.Value = userName;
            userComponent.TotalCount.Value = totalCount;
            userComponent.WinCount.Value = winCount;
        });
    }
}
