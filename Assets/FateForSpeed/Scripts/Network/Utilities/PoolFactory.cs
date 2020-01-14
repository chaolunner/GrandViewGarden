using System.Collections.Generic;
using UnityEngine;
using UniEasy.ECS;
using UniEasy.DI;

public interface IPoolFactory
{
    void Create(PoolData id);
    PoolData Create(GameObject prefab, Transform parent, bool worldPositionStays = false);
    PoolData Create(GameObject prefab, int userId, bool worldPositionStays = false);
    IEntity Pop(GameObject prefab, bool autoCreate = true);
    IEntity Pop(GameObject prefab, int tickId, bool autoCreate = true);
    void Push(IEntity entity);
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
    private Dictionary<GameObject, PoolData> dataDict = new Dictionary<GameObject, PoolData>();
    private Dictionary<IEntity, GameObject> prefabDict = new Dictionary<IEntity, GameObject>();

    public void Create(PoolData data)
    {
        if (!poolDict.ContainsKey(data.Prefab))
        {
            var pool = new Pool(data.Prefab, PrefabFactory, NetworkPrefabFactory);
            poolDict.Add(data.Prefab, pool);
        }
        if (!dataDict.ContainsKey(data.Prefab))
        {
            dataDict.Add(data.Prefab, data);
        }
        if (data.UserId >= 0)
        {
            poolDict[data.Prefab].Create(data.UserId, data.WorldPositionStays);
        }
        else
        {
            poolDict[data.Prefab].Create(data.Parent, data.WorldPositionStays);
        }
    }

    public PoolData Create(GameObject prefab, Transform parent, bool worldPositionStays = false)
    {
        var id = new PoolData(prefab, parent, worldPositionStays);
        Create(id);
        return id;
    }

    public PoolData Create(GameObject prefab, int userId, bool worldPositionStays = false)
    {
        var id = new PoolData(prefab, userId, worldPositionStays);
        Create(id);
        return id;
    }

    public IEntity Pop(GameObject prefab, bool autoCreate = true)
    {
        return Pop(prefab, -1, autoCreate);
    }

    public IEntity Pop(GameObject prefab, int tickId, bool autoCreate = true)
    {
        IEntity entity = null;
        if (poolDict.ContainsKey(prefab))
        {
            entity = poolDict[prefab].Pop(tickId);
        }
        if (entity == null && autoCreate && dataDict.ContainsKey(prefab))
        {
            Create(dataDict[prefab]);
            entity = poolDict[prefab].Pop(tickId);
        }
        if (entity != null)
        {
            prefabDict.Add(entity, prefab);
        }
        return entity;
    }

    public void Push(IEntity entity)
    {
        if (prefabDict.ContainsKey(entity))
        {
            var prefab = prefabDict[entity];

            prefabDict.Remove(entity);

            if (poolDict.ContainsKey(prefab))
            {
                poolDict[prefab].Push(entity);
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
