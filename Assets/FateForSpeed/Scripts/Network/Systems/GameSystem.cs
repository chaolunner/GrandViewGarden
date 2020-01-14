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

        NetwrokTimeline.OnForward(data =>
        {
            for (int i = 0; i < data.UserInputData[0].Length; i++)
            {
                for (int j = 0; j < data.UserInputData[0][i].Inputs.Length; j++)
                {
                    var userId = data.UserInputData[0][i].UserId;
                    var eventInput = data.UserInputData[0][i].GetInput<EventInput>(j);
                    if (eventInput.Type == EventCode.GameStart)
                    {
                        var isRoomOwner = bool.Parse(eventInput.Message);
                        if (!HasUser(userId))
                        {
                            var go = NetworkPrefabFactory.Instantiate(userId, data.TickId, NetworkPlayerPrefab, true);
                            if (isRoomOwner)
                            {
                                go.transform.position += new Vector3(2, 0, -5);
                                go.transform.rotation = Quaternion.Euler(0, -90, 0);
                            }
                            else
                            {
                                go.transform.position += new Vector3(-2, 0, -5);
                                go.transform.rotation = Quaternion.Euler(0, 90, 0);
                            }
                        }
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

    private bool HasUser(int userId)
    {
        for (int i = 0; i < NetworkPlayerComponents.Entities.Count; i++)
        {
            if (NetworkPlayerComponents.Entities[i].GetComponent<NetworkIdentityComponent>().Identity.UserId == userId)
            {
                return true;
            }
        }
        return false;
    }
}
