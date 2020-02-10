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
    private IdentificationObject LobbyId;
    [SerializeField]
    private IdentificationObject RoomId;

    private IGroup LobbyComponents;
    private IGroup UserComponents;
    private IGroup RoomItemComponents;

    private const string TotalCount1Str = "Total Count : ";
    private const string WinCount1Str = "Win Count : ";
    private const string TotalCount2Str = "Total Count\n";
    private const string WinCount2Str = "Win Count\n";
    private const string JoinFailFeedback = "Failed to join the room!";

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

                NetworkSystem.Receive<string>(RequestCode.QuitRoom).Subscribe(data =>
                {
                    if (userComponent.UserId == int.Parse(data) && (userComponent.IsLocalPlayer || userComponent.IsRoomOwner.Value))
                    {
                        var evt = new TriggerEnterEvent();
                        evt.Source = LobbyId;
                        EventSystem.Send(evt);
                    }
                }).AddTo(this.Disposer).AddTo(lobbyComponent.Disposer).AddTo(userComponent.Disposer);
            }).AddTo(this.Disposer).AddTo(lobbyComponent.Disposer);

            NetworkSystem.Receive<string>(RequestCode.CreateRoom).Subscribe(data =>
            {
                string[] strs = data.Split(Separator);
                ReturnCode returnCode = (ReturnCode)int.Parse(strs[0]);
                if (returnCode == ReturnCode.Success)
                {
                    int userId = int.Parse(strs[1]);
                    string username = strs[2];
                    int totalCount = int.Parse(strs[3]);
                    int winCount = int.Parse(strs[4]);
                    EventSystem.Send(new SpawnUserEvent(userId, username, totalCount, winCount, true, true));
                    var evt = new TriggerEnterEvent();
                    evt.Source = RoomId;
                    EventSystem.Send(evt);
                }
            }).AddTo(this.Disposer).AddTo(lobbyComponent.Disposer);

            NetworkSystem.Receive<string>(RequestCode.ListRooms).Subscribe(data =>
            {
                foreach (var entity3 in RoomItemComponents.Entities)
                {
                    var viewComponent = entity3.GetComponent<ViewComponent>();
                    Destroy(viewComponent.Transforms[0].gameObject);
                }

                if (!string.IsNullOrEmpty(data))
                {
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
                }
            }).AddTo(this.Disposer).AddTo(lobbyComponent.Disposer);

            NetworkSystem.Receive<string>(RequestCode.JoinRoom).Subscribe(data =>
            {
                string[] str1s = data.Split(VerticalBar);
                ReturnCode returnCode = (ReturnCode)int.Parse(str1s[0]);
                if (returnCode == ReturnCode.Success)
                {
                    for (int i = 1; i < str1s.Length; i++)
                    {
                        string[] str2s = str1s[i].Split(Separator);
                        int userId = int.Parse(str2s[0]);
                        string username = str2s[1];
                        int totalCount = int.Parse(str2s[2]);
                        int winCount = int.Parse(str2s[3]);
                        bool isRoomOwner = bool.Parse(str2s[4]);
                        EventSystem.Send(new SpawnUserEvent(userId, username, totalCount, winCount, false, isRoomOwner));
                    }
                    var evt = new TriggerEnterEvent();
                    evt.Source = RoomId;
                    EventSystem.Send(evt);
                }
                else
                {
                    EventSystem.Send(new MessageEvent(JoinFailFeedback, LogType.Warning));
                }
            }).AddTo(this.Disposer).AddTo(lobbyComponent.Disposer);
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
