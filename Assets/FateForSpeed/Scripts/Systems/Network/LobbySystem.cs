using UniRx.Triggers;
using UnityEngine;
using UniEasy.ECS;
using UniEasy.Net;
using UniEasy.DI;
using UniEasy;
using Common;
using UniRx;

public class LobbySystem : SystemBehaviour
{
    [Inject]
    private INetworkSystem NetworkSystem;

    [SerializeField]
    private GameObject roomItemPrefab;
    [SerializeField]
    private IdentificationObject JoinRoomIdentifier;

    private IGroup LobbyComponents;
    private IGroup UserComponents;

    private const string EmptySyr = "";
    private const string TotalCountStr = "Total Count : ";
    private const string WinCountStr = "Win Count : ";

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        LobbyComponents = this.Create(typeof(LobbyComponent));
        UserComponents = this.Create(typeof(UserComponent));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        LobbyComponents.OnAdd().Subscribe(entity1 =>
        {
            var lobbyComponent = entity1.GetComponent<LobbyComponent>();

            lobbyComponent.CreateRoomButton.OnPointerClickAsObservable().Subscribe(_ =>
            {
                NetworkSystem.Publish(RequestCode.CreateRoom, EmptySyr);
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
                        lobbyComponent.TotalCountText.text = TotalCountStr + count;
                    }).AddTo(this.Disposer).AddTo(lobbyComponent.Disposer).AddTo(userComponent.Disposer);

                    userComponent.WinCount.DistinctUntilChanged().Where(_ => lobbyComponent.WinCountText).Subscribe(count =>
                    {
                        lobbyComponent.WinCountText.text = WinCountStr + count;
                    }).AddTo(this.Disposer).AddTo(lobbyComponent.Disposer).AddTo(userComponent.Disposer);

                    NetworkSystem.OnEvent(RequestCode.CreateRoom, data =>
                    {
                        ReturnCode returnCode = (ReturnCode)int.Parse(data);
                        if (returnCode == ReturnCode.Success)
                        {
                            PrefabFactory.Instantiate(roomItemPrefab, lobbyComponent.RoomItemRoot, false, go =>
                            {
                                go.GetComponent<RoomItemComponent>().UserEntities.Add(entity2);
                            });
                            var evt = new TriggerEnterEvent();
                            evt.Source = JoinRoomIdentifier;
                            EventSystem.Send(evt);
                        }
                    });
                }
            }).AddTo(this.Disposer).AddTo(lobbyComponent.Disposer);
        }).AddTo(this.Disposer);
    }
}
