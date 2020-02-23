using System.Collections.Generic;
using UnityEngine;
using UniEasy.ECS;
using UniEasy.DI;

public interface IPoolFactory
{
    IEntity Spawn(GameObject prefab, Transform parent = null, bool worldPositionStays = false);
    IEntity Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null);
    IEntity Spawn(GameObject prefab, int userId, int tickId, Transform parent = null, bool worldPositionStays = false);
    IEntity Spawn(GameObject prefab, int userId, int tickId, Vector3 position, Quaternion rotation, Transform parent = null);
    void Despawn(IEntity entity);
    /// <param name="force">Whether to destroy unrecycled game objects.</param>
    void Destroy(GameObject prefab, bool force = false);
}

public class PoolFactory : IPoolFactory
{
    [Inject]
    private PrefabFactory PrefabFactory;
    [Inject]
    private NetworkPrefabFactory NetworkPrefabFactory;

    private Dictionary<GameObject, IPool> poolDict = new Dictionary<GameObject, IPool>();
    private Dictionary<IEntity, GameObject> prefabDict = new Dictionary<IEntity, GameObject>();

    private void Create(GameObject prefab, int userId, int tickId, Vector3 position, Quaternion rotation, Transform parent, bool worldPositionStays)
    {
        if (!poolDict.ContainsKey(prefab))
        {
            var pool = new Pool(prefab, PrefabFactory, NetworkPrefabFactory);
            poolDict.Add(prefab, pool);
        }
        if (worldPositionStays)
        {
            poolDict[prefab].Create(userId, tickId, parent, true);
        }
        else
        {
            poolDict[prefab].Create(userId, tickId, position, rotation, parent);
        }
    }

    public IEntity Spawn(GameObject prefab, Transform parent = null, bool worldPositionStays = false)
    {
        return Spawn(prefab, -1, -1, Vector3.zero, Quaternion.identity, parent, worldPositionStays);
    }

    public IEntity Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        return Spawn(prefab, -1, -1, position, rotation, parent, false);
    }

    public IEntity Spawn(GameObject prefab, int userId, int tickId, Transform parent = null, bool worldPositionStays = false)
    {
        return Spawn(prefab, userId, tickId, Vector3.zero, Quaternion.identity, parent, worldPositionStays);
    }

    public IEntity Spawn(GameObject prefab, int userId, int tickId, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        return Spawn(prefab, userId, tickId, position, rotation, parent, false);
    }

    private IEntity Spawn(GameObject prefab, int userId, int tickId, Vector3 position, Quaternion rotation, Transform parent, bool worldPositionStays)
    {
        IEntity entity = null;
        if (poolDict.ContainsKey(prefab))
        {
            entity = poolDict[prefab].Spawn(tickId);
        }
        if (entity == null)
        {
            Create(prefab, userId, tickId, position, rotation, parent, worldPositionStays);
            entity = poolDict[prefab].Spawn(tickId);
        }
        else
        {
            var viewComponent = entity.GetComponent<ViewComponent>();

            viewComponent.Transforms[0].SetParent(parent);
            viewComponent.Transforms[0].position = position;
            viewComponent.Transforms[0].rotation = rotation;
        }
        if (entity != null)
        {
            prefabDict.Add(entity, prefab);
        }
        return entity;
    }

    public void Despawn(IEntity entity)
    {
        if (prefabDict.ContainsKey(entity))
        {
            var prefab = prefabDict[entity];

            prefabDict.Remove(entity);

            if (poolDict.ContainsKey(prefab))
            {
                poolDict[prefab].Despawn(entity);
            }
        }
    }

    public void Destroy(GameObject prefab, bool force = false)
    {
        if (poolDict.ContainsKey(prefab))
        {
            poolDict[prefab].Destroy(force);
        }
    }
}
