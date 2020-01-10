using System.Collections.Generic;
using UnityEngine;
using UniEasy.ECS;
using UniEasy.DI;

public interface IPoolFactory
{
    void Create(PoolId id);
    PoolId Create(GameObject prefab, Transform parent, bool worldPositionStays = false);
    PoolId Create(GameObject prefab, int userId, bool worldPositionStays = false);
    GameObject Pop(GameObject prefab, bool autoCreate = true);
    void Push(GameObject prefab, GameObject go);
    /// <param name="force">Whether to destroy unrecycled game objects.</param>
    void Destroy(GameObject prefab, bool force = false);
}

public class PoolFactory : IPoolFactory
{
    [Inject]
    private PrefabFactory PrefabFactory;
    [Inject]
    private NetworkPrefabFactory NetworkPrefabFactory;

    private Dictionary<PoolId, IPool> poolDict = new Dictionary<PoolId, IPool>();
    private Dictionary<GameObject, List<PoolId>> idDict = new Dictionary<GameObject, List<PoolId>>();

    public void Create(PoolId id)
    {
        if (!poolDict.ContainsKey(id))
        {
            var pool = new Pool(id.Prefab, PrefabFactory, NetworkPrefabFactory);
            poolDict.Add(id, pool);
        }
        if (!idDict.ContainsKey(id.Prefab))
        {
            idDict.Add(id.Prefab, new List<PoolId>());
        }
        if (!idDict[id.Prefab].Contains(id))
        {
            idDict[id.Prefab].Add(id);
        }
        if (id.UserId >= 0)
        {
            poolDict[id].Create(id.UserId, id.WorldPositionStays);
        }
        else
        {
            poolDict[id].Create(id.Parent, id.WorldPositionStays);
        }
    }

    public PoolId Create(GameObject prefab, Transform parent, bool worldPositionStays = false)
    {
        var id = new PoolId(prefab, parent, worldPositionStays);
        Create(id);
        return id;
    }

    public PoolId Create(GameObject prefab, int userId, bool worldPositionStays = false)
    {
        var id = new PoolId(prefab, userId, worldPositionStays);
        Create(id);
        return id;
    }

    public GameObject Pop(GameObject prefab, bool autoCreate = true)
    {
        GameObject go = null;
        if (idDict.ContainsKey(prefab) && poolDict.ContainsKey(idDict[prefab][0]))
        {
            go = poolDict[idDict[prefab][0]].Pop();
        }
        if (go == null && autoCreate && idDict.ContainsKey(prefab))
        {
            Create(idDict[prefab][0]);
            go = Pop(idDict[prefab][0].Prefab, autoCreate);
        }
        return go;
    }

    public void Push(GameObject prefab, GameObject go)
    {
        if (idDict.ContainsKey(prefab) && poolDict.ContainsKey(idDict[prefab][0]))
        {
            poolDict[idDict[prefab][0]].Push(go);
        }
    }

    public void Destroy(GameObject prefab, bool force = false)
    {
        if (idDict.ContainsKey(prefab) && poolDict.ContainsKey(idDict[prefab][0]))
        {
            poolDict[idDict[prefab][0]].Destroy(force);
        }
    }
}
