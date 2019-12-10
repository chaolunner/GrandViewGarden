using UnityEngine;
using UniEasy.ECS;
using UniEasy;
using Common;
using UniRx;

public class GameSystem : NetworkSystemBehaviour
{
    [SerializeField]
    private GameObject NetworkPlayerPrefab;
    [SerializeField]
    private IdentificationObject GameStartId;
    private IGroup UserComponents;
    private IGroup NetworkPlayerComponents;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        UserComponents = this.Create(typeof(UserComponent), typeof(ViewComponent));
        NetworkPlayerComponents = this.Create(typeof(NetworkPlayerComponent));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        NetworkSystem.Receive<string>(RequestCode.StartGame).Subscribe(data =>
        {
            int userId = int.Parse(data);
            NetworkPrefabFactory.Instantiate(userId, NetworkPlayerPrefab, true);
        }).AddTo(this.Disposer);

        NetworkPlayerComponents.OnAdd().Subscribe(entity =>
        {
            if (NetworkPlayerComponents.Entities.Count == UserComponents.Entities.Count)
            {
                var evt = new TriggerEnterEvent();
                evt.Source = GameStartId;
                EventSystem.Send(evt);
            }
        }).AddTo(this.Disposer);
    }
}
