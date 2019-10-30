using UnityEngine;
using UniEasy.ECS;
using UniRx;
using TMPro;

public class LobbySystem : SystemBehaviour
{
    [SerializeField] private TextMeshProUGUI UsernameText;
    [SerializeField] private TextMeshProUGUI TotalCountText;
    [SerializeField] private TextMeshProUGUI WinCountText;

    private IGroup UserComponents;

    private const string TotalCountStr = "Total Count : ";
    private const string WinCountStr = "Win Count : ";

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        UserComponents = this.Create(typeof(UserComponent));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        UserComponents.OnAdd().Subscribe(entity =>
        {
            var userComponent = entity.GetComponent<UserComponent>();

            userComponent.Username.DistinctUntilChanged().Where(_ => UsernameText && userComponent.IsLocalPlayer).Subscribe(name =>
            {
                UsernameText.text = name;
            }).AddTo(this.Disposer).AddTo(userComponent.Disposer);

            userComponent.TotalCount.DistinctUntilChanged().Where(_ => TotalCountText && userComponent.IsLocalPlayer).Subscribe(count =>
            {
                TotalCountText.text = TotalCountStr + count;
            }).AddTo(this.Disposer).AddTo(userComponent.Disposer);

            userComponent.WinCount.DistinctUntilChanged().Where(_ => WinCountText && userComponent.IsLocalPlayer).Subscribe(count =>
            {
                WinCountText.text = WinCountStr + count;
            }).AddTo(this.Disposer).AddTo(userComponent.Disposer);
        }).AddTo(this.Disposer);
    }
}
