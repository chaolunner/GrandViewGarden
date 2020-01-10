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
    private NetworkGroup Network;
    private INetworkTimeline NetwrokTimeline;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);
        UserComponents = this.Create(typeof(UserComponent), typeof(ViewComponent));
        NetworkPlayerComponents = this.Create(typeof(NetworkPlayerComponent), typeof(NetworkIdentityComponent));
        Network = LockstepFactory.Create(fixedDeltaTime: 0);
        NetwrokTimeline = Network.CreateTimeline(typeof(EventInput));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        NetwrokTimeline.OnReverse(result =>
        {
        }).AddTo(this.Disposer);

        NetwrokTimeline.OnForward((UserInputData[][] userInputData, float deltaTime, int tickId) =>
        {
            for (int i = 0; i < userInputData.Length; i++)
            {
                for (int j = 0; j < userInputData[i].Length; j++)
                {
                    var eventInput = userInputData[i][j].Input as EventInput;
                    var strs = eventInput.Read(EventCode.GameStart);
                    if (strs != null)
                    {
                        int userId = int.Parse(strs[0]);
                        NetworkPrefabFactory.Instantiate(userId, tickId, NetworkPlayerPrefab, true);
                    }
                }
            }

            return null;
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
