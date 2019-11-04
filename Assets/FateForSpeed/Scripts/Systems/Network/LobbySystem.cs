using UniRx.Triggers;
using UnityEngine;
using UniEasy.ECS;
using UniEasy.Net;
using UniEasy;
using Common;
using UniRx;

public class LobbySystem : NetworkSystemBehaviour
{
    [SerializeField]
    private GameObject roomItemPrefab;
    [SerializeField]
    private IdentificationObject JoinRoomIdentifier;

    private IGroup LobbyComponents;
    private IGroup UserComponents;
    private IGroup RoomItemComponents;

    private const string TotalCount1Str = "Total Count : ";
    private const string WinCount1Str = "Win Count : ";
    private const string TotalCount2Str = "Total Count\n";
    private const string WinCount2Str = "Win Count\n";

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        LobbyComponents = this.Create(typeof(LobbyComponent));
        UserComponents = this.Create(typeof(UserComponent));
        RoomItemComponents = this.Create(typeof(RoomItemComponent), typeof(ViewComponent));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        LobbyComponents.OnAdd().Subscribe(entity1 =>
        {
            var lobbyComponent = entity1.GetComponent<LobbyComponent>();

            lobbyComponent.CreateRoomButton.OnPointerClickAsObservable().Subscribe(_ =>
            {
                NetworkSystem.Publish(RequestCode.CreateRoom, EmptyStr);
            }).AddTo(this.Disposer).AddTo(lobbyComponent.Disposer);

            lobbyComponent.RefreshButton.OnPointerClickAsObservable().Subscribe(_ =>
            {
                NetworkSystem.Publish(RequestCode.ListRooms, EmptyStr);
            }).AddTo(this.Disposer).AddTo(lobbyComponent.Disposer);

            UserComponents.OnAdd().Subscribe(entity2 =>
            {
                var userComponent = entity2.GetComponent<UserComponent>();

                if (userComponent.IsLocalPlayer)
                {
                    userComponent.Username.DistinctUntilChanged().Where(_ => lobbyComponent.UsernameText).Subscribe(name =>
                    {
                        lobbyComponent.UsernameText.text = name;
                    }).AddTo(this.Disposer).AddTo(lobbyComponent.Disposer).AddTo(userComponent.Disposer);

                    userComponent.TotalCount.DistinctUntilChanged().Where(_ => lobbyComponent.TotalCountText).Subscribe(count =>
                    {
                        lobbyComponent.TotalCountText.text = TotalCount1Str + count;
                    }).AddTo(this.Disposer).AddTo(lobbyComponent.Disposer).AddTo(userComponent.Disposer);

                    userComponent.WinCount.DistinctUntilChanged().Where(_ => lobbyComponent.WinCountText).Subscribe(count =>
                    {
                        lobbyComponent.WinCountText.text = WinCount1Str + count;
                    }).AddTo(this.Disposer).AddTo(lobbyComponent.Disposer).AddTo(userComponent.Disposer);
                }
            }).AddTo(this.Disposer).AddTo(lobbyComponent.Disposer);

            NetworkSystem.OnEvent(RequestCode.CreateRoom, data =>
            {
                ReturnCode returnCode = (ReturnCode)int.Parse(data);
                if (returnCode == ReturnCode.Success)
                {
                    NetworkSystem.Publish(RequestCode.ListRooms, EmptyStr);
                    var evt = new TriggerEnterEvent();
                    evt.Source = JoinRoomIdentifier;
                    EventSystem.Send(evt);
                }
            });

            NetworkSystem.OnEvent(RequestCode.ListRooms, data =>
            {
                foreach (var e in RoomItemComponents.Entities)
                {
                    var viewComponent = e.GetComponent<ViewComponent>();
                    Destroy(viewComponent.Transforms[0].gameObject);
                }

                string[] str1s = data.Split(VerticalBar);
                foreach (var str1 in str1s)
                {
                    string[] str2s = str1.Split(Separator);
                    PrefabFactory.Instantiate(roomItemPrefab, lobbyComponent.RoomItemRoot, false, go =>
                    {
                        var roomItemComponent = go.GetComponent<RoomItemComponent>();

                        roomItemComponent.UserId = int.Parse(str2s[0]);
                        roomItemComponent.UsernameText.text = str2s[1];
                        roomItemComponent.TotalCountText.text = TotalCount2Str + str2s[2];
                        roomItemComponent.WinCountText.text = WinCount2Str + str2s[3];
                    });
                }
            });
        }).AddTo(this.Disposer);

        RoomItemComponents.OnAdd().Subscribe(entity =>
        {
            var roomItemComponent = entity.GetComponent<RoomItemComponent>();

            roomItemComponent.JoinButton.OnPointerClickAsObservable().Subscribe(_ =>
            {
                NetworkSystem.Publish(RequestCode.JoinRoom, roomItemComponent.UserId.ToString());
            }).AddTo(this.Disposer).AddTo(roomItemComponent.Disposer);
        }).AddTo(this.Disposer);
    }
}
