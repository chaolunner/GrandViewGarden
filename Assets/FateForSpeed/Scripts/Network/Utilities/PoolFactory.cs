using System.Collections.Generic;
using UnityEngine;
using UniEasy.ECS;
using UniEasy.DI;

public interface IPoolFactory
{
    void Create(GameObject prefab, Transform parent, bool worldPositionStays = false);
    void Create(int userId, GameObject prefab, bool worldPositionStays = false);
    GameObject Pop(GameObject prefab);
    void Push(GameObject prefab, GameObject go);
    /// <param name="force">Whether to destroy unrecycled game objects.</param>
    void Destroy(GameObject prefab, bool force = false);
}

public class PoolFactory : IPoolFactory
{
    [Inject]
    public PrefabFactory PrefabFactory;
    [Inject]
    public NetworkPrefabFactory NetworkPrefabFactory;

    private Dictionary<GameObject, IPool> poolDict = new Dictionary<GameObject, IPool>();

    public void Create(GameObject prefab, Transform parent, bool worldPositionStays = false)
    {
        if (!poolDict.ContainsKey(prefab))
        {
            var pool = new Pool(prefab, PrefabFactory, NetworkPrefabFactory);
            poolDict.Add(prefab, pool);
        }
        poolDict[prefab].Create(parent, worldPositionStays);
    }

    public void Create(int userId, GameObject prefab, bool worldPositionStays = false)
    {
        if (!poolDict.ContainsKey(prefab))
        {
            var pool = new Pool(prefab, PrefabFactory, NetworkPrefabFactory);
            poolDict.Add(prefab, pool);
        }
        poolDict[prefab].Create(userId, worldPositionStays);
    }

    public GameObject Pop(GameObject prefab)
    {
        if (poolDict.ContainsKey(prefab))
        {
            return poolDict[prefab].Pop();
        }
        return null;
    }

    public void Push(GameObject prefab, GameObject go)
    {
        if (poolDict.ContainsKey(prefab))
        {
            poolDict[prefab].Push(go);
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
