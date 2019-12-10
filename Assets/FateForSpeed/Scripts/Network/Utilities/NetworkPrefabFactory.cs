using System.Collections.Generic;
using UnityEngine;
using UniEasy.ECS;
using UniEasy.DI;
using UniEasy;

public class NetworkPrefabFactory
{
    private GroupFactory GroupFactory;
    private PrefabFactory PrefabFactory;
    private IGroup UserComponents;
    private Dictionary<int, SequentialIdentityGenerator> identityDict = new Dictionary<int, SequentialIdentityGenerator>();

    private int GenerateId(int userId)
    {
        if (!identityDict.ContainsKey(userId))
        {
            identityDict.Add(userId, new SequentialIdentityGenerator());
        }
        return identityDict[userId].GenerateId();
    }

    [Inject]
    public NetworkPrefabFactory(GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        GroupFactory = groupFactory;
        PrefabFactory = prefabFactory;
        UserComponents = GroupFactory.Create(typeof(UserComponent), typeof(ViewComponent));
    }

    public GameObject Instantiate(int userId, int instanceId, GameObject prefab, bool worldPositionStays = false)
    {
        foreach (var entity in UserComponents.Entities)
        {
            var userComponent = entity.GetComponent<UserComponent>();
            var viewComponent = entity.GetComponent<ViewComponent>();

            if (userId == userComponent.UserId)
            {
                return PrefabFactory.Instantiate(prefab, viewComponent.Transforms[0], worldPositionStays, go =>
                {
                    var networkIdentityComponent = go.GetComponent<NetworkIdentityComponent>();
                    networkIdentityComponent.Identity = new NetworkId(userId, instanceId);
                    networkIdentityComponent.IsLocalPlayer = userComponent.IsLocalPlayer;
                });
            }
        }
        return null;
    }

    public GameObject Instantiate(int userId, GameObject prefab, bool worldPositionStays = false)
    {
        return Instantiate(userId, GenerateId(userId), prefab, worldPositionStays);
    }
}
