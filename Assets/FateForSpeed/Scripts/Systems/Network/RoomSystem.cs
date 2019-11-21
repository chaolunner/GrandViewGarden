using UniEasy.ECS;
using System.Linq;
using Common;
using UniRx;

public class RoomSystem : NetworkSystemBehaviour
{
    private IGroup RoomComponents;
    private IGroup UserComponents;

    private const string WaitingToJoinStr = "Waiting for players to join...";
    private const string TotalCountStr = "Total Count : ";
    private const string WinCountStr = "Win Count : ";

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        RoomComponents = this.Create(typeof(RoomComponent));
        UserComponents = this.Create(typeof(UserComponent));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        RoomComponents.OnAdd().Subscribe(entity1 =>
        {
            var roomComponent = entity1.GetComponent<RoomComponent>();

            UserComponents.Entities.OnAdd().Subscribe(entity2 =>
            {
                var userComponent = entity2.GetComponent<UserComponent>();

                userComponent.IsRoomOwner.DistinctUntilChanged().Subscribe(_ =>
                {
                    UpdateRoomState(roomComponent);
                }).AddTo(this.Disposer).AddTo(userComponent.Disposer);
            }).AddTo(this.Disposer);

            UserComponents.Entities.OnRemove().Select(_ => true)
                .Merge(Observable.ReturnUnit().Select(_ => true))
                .Subscribe(_ =>
            {
                UpdateRoomState(roomComponent);
            }).AddTo(this.Disposer);

            roomComponent.StartButton.OnClickAsObservable().Subscribe(_ =>
            {
                foreach (var entity3 in UserComponents.Entities)
                {
                    var userComponent = entity3.GetComponent<UserComponent>();
                    if (userComponent.IsLocalPlayer)
                    {
                        NetworkSystem.Publish(RequestCode.StartGame, userComponent.UserId.ToString());
                        break;
                    }
                }
            }).AddTo(this.Disposer).AddTo(roomComponent.Disposer);

            roomComponent.ExitButton.OnClickAsObservable().Subscribe(_ =>
            {
                foreach (var entity4 in UserComponents.Entities)
                {
                    var userComponent = entity4.GetComponent<UserComponent>();
                    if (userComponent.IsLocalPlayer)
                    {
                        NetworkSystem.Publish(RequestCode.QuitRoom, userComponent.UserId.ToString());
                        break;
                    }
                }
            }).AddTo(this.Disposer).AddTo(roomComponent.Disposer);
        }).AddTo(this.Disposer);
    }

    private void UpdateRoomState(RoomComponent roomComponent)
    {
        roomComponent.User1NameText.text = WaitingToJoinStr;
        roomComponent.User1TotalCountText.text = TotalCountStr + 0;
        roomComponent.User1WinCountText.text = WinCountStr + 0;
        roomComponent.User2NameText.text = WaitingToJoinStr;
        roomComponent.User2TotalCountText.text = TotalCountStr + 0;
        roomComponent.User2WinCountText.text = WinCountStr + 0;

        foreach (var entity in UserComponents.Entities)
        {
            var userComponent = entity.GetComponent<UserComponent>();

            if (userComponent.IsRoomOwner.Value)
            {
                userComponent.Username.DistinctUntilChanged().Where(_ => roomComponent.User1NameText).Subscribe(name =>
                {
                    roomComponent.User1NameText.text = name;
                }).AddTo(this.Disposer).AddTo(roomComponent.Disposer).AddTo(userComponent.Disposer);

                userComponent.TotalCount.DistinctUntilChanged().Where(_ => roomComponent.User1TotalCountText).Subscribe(count =>
                {
                    roomComponent.User1TotalCountText.text = TotalCountStr + count;
                }).AddTo(this.Disposer).AddTo(roomComponent.Disposer).AddTo(userComponent.Disposer);

                userComponent.WinCount.DistinctUntilChanged().Where(_ => roomComponent.User1WinCountText).Subscribe(count =>
                {
                    roomComponent.User1WinCountText.text = WinCountStr + count;
                }).AddTo(this.Disposer).AddTo(roomComponent.Disposer).AddTo(userComponent.Disposer);
            }
            else if (UserComponents.Entities.Count > 1)
            {
                userComponent.Username.DistinctUntilChanged().Where(_ => roomComponent.User2NameText).Subscribe(name =>
                {
                    roomComponent.User2NameText.text = name;
                }).AddTo(this.Disposer).AddTo(roomComponent.Disposer).AddTo(userComponent.Disposer);

                userComponent.TotalCount.DistinctUntilChanged().Where(_ => roomComponent.User2TotalCountText).Subscribe(count =>
                {
                    roomComponent.User2TotalCountText.text = TotalCountStr + count;
                }).AddTo(this.Disposer).AddTo(roomComponent.Disposer).AddTo(userComponent.Disposer);

                userComponent.WinCount.DistinctUntilChanged().Where(_ => roomComponent.User2WinCountText).Subscribe(count =>
                {
                    roomComponent.User2WinCountText.text = WinCountStr + count;
                }).AddTo(this.Disposer).AddTo(roomComponent.Disposer).AddTo(userComponent.Disposer);
            }
        }
    }
}
