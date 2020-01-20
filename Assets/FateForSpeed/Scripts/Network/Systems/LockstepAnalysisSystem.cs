using System.Collections.Generic;
using UnityEngine;
using UniEasy.ECS;
using System.Text;
using Common;
using UniRx;

/// <summary>
/// Lockstep deterministic monitoring system.
/// </summary>
public class LockstepAnalysisSystem : NetworkSystemBehaviour
{
    public int Interval = 1000;

    private NetworkGroup Network;
    private INetworkTimeline NetwrokTimeline;
    private IGroup NetworkIdentities;
    private Dictionary<int, string> analyzedDataDict = new Dictionary<int, string>();

    private const string DataMatchSuccess = "Data match successful, lockstep is normal.";
    private const string DataMatchFailed = "Data match failed, lockstep is abnormal!";
    private const string InsufficientDataWarning = "Insufficient data to complete analysis.";
    private const string DataError = "Data error, analysis failed.";

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);
        Network = LockstepFactory.Create();
        NetwrokTimeline = Network.CreateTimeline(typeof(EventInput));
        NetworkIdentities = this.Create(typeof(NetworkIdentityComponent), typeof(ViewComponent));
    }

    public override void OnEnable()
    {
        base.OnEnable();
        NetwrokTimeline.OnForward(data =>
        {
            if (data.TickId % Interval == 0)
            {
                string msg = PackMessage(data.TickId);
                analyzedDataDict.Add(data.TickId, msg);
                NetworkSystem.Publish(RequestCode.LockstepAnalysis, msg);
            }
            return null;
        }).AddTo(this.Disposer);
        NetworkSystem.Receive<string>(RequestCode.LockstepAnalysis).Subscribe(data =>
        {
            string[] str1s = data.Split(VerticalBar);
            int tickId;
            if (str1s.Length == 2 && int.TryParse(str1s[0], out tickId))
            {
                if (analyzedDataDict.ContainsKey(tickId))
                {
                    if (analyzedDataDict[tickId] == str1s[1])
                    {
                        Debug.Log(DataMatchSuccess);
                    }
                    else
                    {
                        Debug.LogError(DataMatchFailed);
                    }
                }
                else
                {
                    Debug.LogWarning(InsufficientDataWarning);
                }
            }
            else
            {
                Debug.LogError(DataError);
            }
        }).AddTo(this.Disposer);
    }

    private string PackMessage(int tickId)
    {
        var msgBuilder = new StringBuilder(tickId.ToString() + VerticalBar);
        var dict = new Dictionary<NetworkId, string>();
        for (int i = 0; i < NetworkIdentities.Entities.Count; i++)
        {
            var networkIdentityComponent = NetworkIdentities.Entities[i].GetComponent<NetworkIdentityComponent>();
            var viewComponent = NetworkIdentities.Entities[i].GetComponent<ViewComponent>();
            if (networkIdentityComponent.TickIdWhenCreated < 0) { continue; }
            if (!dict.ContainsKey(networkIdentityComponent.Identity))
            {
                dict.Add(networkIdentityComponent.Identity, EmptyStr);
            }
            dict[networkIdentityComponent.Identity] += viewComponent.Transforms[0].position.x.ToString() + Separator + viewComponent.Transforms[0].position.y.ToString() + Separator + viewComponent.Transforms[0].position.z.ToString() + Separator + viewComponent.Transforms[0].eulerAngles.x.ToString() + Separator + viewComponent.Transforms[0].eulerAngles.y.ToString() + Separator + viewComponent.Transforms[0].eulerAngles.z.ToString();
        }
        var list = new List<NetworkId>(dict.Keys);
        list.Sort();
        for (int i = 0; i < list.Count; i++)
        {
            msgBuilder.Append(dict[list[i]] + Separator);
        }
        msgBuilder.Remove(msgBuilder.Length - 1, 1);
        return msgBuilder.ToString();
    }
}
