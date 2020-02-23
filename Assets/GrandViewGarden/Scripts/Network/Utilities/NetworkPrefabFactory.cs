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

    public GameObject Instantiate(int userId, int tickId, GameObject prefab, Transform parent = null, bool worldPositionStays = false)
    {
        return Instantiate(userId, tickId, GenerateId(userId), prefab, Vector3.zero, Quaternion.identity, parent, worldPositionStays);
    }

    public GameObject Instantiate(int userId, int tickId, GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        return Instantiate(userId, tickId, GenerateId(userId), prefab, position, rotation, parent, false);
    }

    private GameObject Instantiate(int userId, int tickId, int instanceId, GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, bool worldPositionStays)
    {
        foreach (var entity in UserComponents.Entities)
        {
            var userComponent = entity.GetComponent<UserComponent>();
            var viewComponent = entity.GetComponent<ViewComponent>();

            if (userId == userComponent.UserId)
            {
                var pos = worldPositionStays ? prefab.transform.position : position;
                var rot = worldPositionStays ? prefab.transform.rotation : rotation;

                return PrefabFactory.Instantiate(prefab, pos, rot, parent ?? viewComponent.Transforms[0], go =>
                {
                    var networkIdentityComponent = go.GetComponent<NetworkIdentityComponent>() ?? go.AddComponent<NetworkIdentityComponent>();

                    networkIdentityComponent.Identity = new NetworkId(userId, instanceId);
                    networkIdentityComponent.IsLocalPlayer = userComponent.IsLocalPlayer;
                    networkIdentityComponent.TickIdWhenCreated = tickId;
                });
            }
        }
        return null;
    }
}
