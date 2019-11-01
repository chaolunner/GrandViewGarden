using UniEasy.ECS;
using UniRx;

public class RoomSystem : SystemBehaviour
{
    private IGroup RoomItemComponents;

    private const string TotalCountStr = "Total Count\n";
    private const string WinCountStr = "Win Count\n";

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        RoomItemComponents = this.Create(typeof(RoomItemComponent));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        RoomItemComponents.OnAdd().Subscribe(entity =>
        {
            var roomItemComponent = entity.GetComponent<RoomItemComponent>();
            var homeowner = roomItemComponent.UserEntities[0].GetComponent<UserComponent>();

            homeowner.Username.DistinctUntilChanged().Where(_ => roomItemComponent.UsernameText).Subscribe(name =>
            {
                roomItemComponent.UsernameText.text = name;
            }).AddTo(this.Disposer).AddTo(roomItemComponent.Disposer).AddTo(homeowner.Disposer);

            homeowner.TotalCount.DistinctUntilChanged().Where(_ => roomItemComponent.TotalCountText).Subscribe(count =>
            {
                roomItemComponent.TotalCountText.text = TotalCountStr + count;
            }).AddTo(this.Disposer).AddTo(roomItemComponent.Disposer).AddTo(homeowner.Disposer);

            homeowner.WinCount.DistinctUntilChanged().Where(_ => roomItemComponent.WinCountText).Subscribe(count =>
            {
                roomItemComponent.WinCountText.text = WinCountStr + count;
            }).AddTo(this.Disposer).AddTo(roomItemComponent.Disposer).AddTo(homeowner.Disposer);
        }).AddTo(this.Disposer);
    }
}
