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

    private const string DataMatchSuccess = "Data match successful, lockstep is normal. [{0}]\n{1}\n{2}";
    private const string DataMatchFailed = "Data match failed, lockstep is abnormal! [{0}]\n{1}\n{2}";
    private const string InsufficientDataWarning = "Insufficient data to complete analysis.";
    private const string DataError = "Data error, analysis failed.";
    private const char LeftSquareBracket = '[';
    private const char RightSquareBracket = ']';
    private const char Newline = '\n';

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);
        Network = LockstepFactory.Create(priority: (int)LockstepSettings.Priority.Debug);
        NetwrokTimeline = Network.CreateTimeline(typeof(EventInput));
        NetworkIdentities = this.Create(typeof(NetworkIdentityComponent), typeof(ViewComponent));
    }

    public override void OnEnable()
    {
        base.OnEnable();
        NetwrokTimeline.OnForward(data =>
        {
            if (NetworkSystem.Mode != SessionMode.Offline && data.TickId % Interval == 0)
            {
                string msg = PackMessage();
                analyzedDataDict.Add(data.TickId, msg);
                NetworkSystem.Publish(RequestCode.LockstepAnalysis, data.TickId.ToString() + VerticalBar + msg);
            }
            return null;
        }).AddTo(this.Disposer);
        NetworkSystem.Receive(RequestCode.LockstepAnalysis).Subscribe(data =>
        {
            int tickId;
            int returnCode;
            string[] strs = data.StringValue.Split(VerticalBar);
            if (strs.Length == 2 && int.TryParse(strs[0], out tickId))
            {
                if (analyzedDataDict.ContainsKey(tickId))
                {
                    if (analyzedDataDict[tickId] == strs[1]) { Debug.Log(string.Format(DataMatchSuccess, tickId, analyzedDataDict[tickId], strs[1])); }
                    else { Debug.LogError(string.Format(DataMatchFailed, tickId, analyzedDataDict[tickId], strs[1])); }
                }
                else { Debug.LogWarning(InsufficientDataWarning); }
            }
            else if (int.TryParse(data.StringValue, out returnCode) && (ReturnCode)returnCode == ReturnCode.Success) { }
            else { Debug.LogError(DataError); }
        }).AddTo(this.Disposer);
    }

    private string PackMessage()
    {
        var msgBuilder = new StringBuilder();
        var dict = new Dictionary<NetworkId, StringBuilder>();
        for (int i = 0; i < NetworkIdentities.Entities.Count; i++)
        {
            var networkIdentityComponent = NetworkIdentities.Entities[i].GetComponent<NetworkIdentityComponent>();
            var viewComponent = NetworkIdentities.Entities[i].GetComponent<ViewComponent>();
            if (networkIdentityComponent.TickIdWhenCreated < 0) { continue; }
            if (!dict.ContainsKey(networkIdentityComponent.Identity)) { dict.Add(networkIdentityComponent.Identity, new StringBuilder()); }
            var builder = dict[networkIdentityComponent.Identity];
            builder.Append(LeftSquareBracket);
            builder.Append(networkIdentityComponent.Identity.UserId);
            builder.Append(Separator);
            builder.Append(networkIdentityComponent.Identity.InstanceId);
            builder.Append(RightSquareBracket);
            builder.Append(LeftSquareBracket);
            builder.Append(viewComponent.Transforms[0].position.x);
            builder.Append(Separator);
            builder.Append(viewComponent.Transforms[0].position.y);
            builder.Append(Separator);
            builder.Append(viewComponent.Transforms[0].position.z);
            builder.Append(RightSquareBracket);
            builder.Append(LeftSquareBracket);
            builder.Append(viewComponent.Transforms[0].eulerAngles.x);
            builder.Append(Separator);
            builder.Append(viewComponent.Transforms[0].eulerAngles.y);
            builder.Append(Separator);
            builder.Append(viewComponent.Transforms[0].eulerAngles.z);
            builder.Append(RightSquareBracket);
        }
        var list = new List<NetworkId>(dict.Keys);
        list.Sort();
        for (int i = 0; i < list.Count; i++) { msgBuilder.Append(dict[list[i]].ToString() + Newline); }
        if (msgBuilder.Length > 0) { msgBuilder.Remove(msgBuilder.Length - 1, 1); }
        return msgBuilder.ToString();
    }
}
