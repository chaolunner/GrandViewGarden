using System.Collections.Generic;
using UnityEngine;
using UniEasy.ECS;

public interface IPool
{
    IEnumerable<IEntity> Entities { get; }
    GameObject Prefab { get; }
    void Create(int userId, int tickId, Transform parent, bool worldPositionStays);
    void Create(int userId, int tickId, Vector3 position, Quaternion rotation, Transform parent);
    IEntity Spawn(int tickId = -1);
    void Despawn(IEntity entity);
    /// <param name="force">Whether to destroy unrecycled game objects.</param>
    void Destroy(bool force);
}

public class Pool : IPool
{
    private GameObject prefab;
    private List<IEntity> entities;
    private List<IEntity> inactiveParts;
    private List<IEntity> activeParts;
    private PrefabFactory PrefabFactory;
    private NetworkPrefabFactory NetworkPrefabFactory;

    public IEnumerable<IEntity> Entities
    {
        get
        {
            entities.Clear();
            entities.AddRange(inactiveParts);
            entities.AddRange(activeParts);
            return entities;
        }
    }

    public GameObject Prefab
    {
        get { return prefab; }
    }

    public Pool(GameObject prefab, PrefabFactory prefabFactory, NetworkPrefabFactory networkPrefabFactory)
    {
        inactiveParts = new List<IEntity>();
        activeParts = new List<IEntity>();
        this.prefab = prefab;
        PrefabFactory = prefabFactory;
        NetworkPrefabFactory = networkPrefabFactory;
    }

    public void Create(int userId, int tickId, Transform parent, bool worldPositionStays)
    {
        Create(userId, tickId, Vector3.zero, Quaternion.identity, parent, worldPositionStays);
    }

    public void Create(int userId, int tickId, Vector3 position, Quaternion rotation, Transform parent)
    {
        Create(userId, tickId, position, rotation, parent, false);
    }

    private void Create(int userId, int tickId, Vector3 position, Quaternion rotation, Transform parent, bool worldPositionStays)
    {
        var go = userId >= 0 ? (worldPositionStays ? NetworkPrefabFactory.Instantiate(userId, tickId, prefab, parent, worldPositionStays) : NetworkPrefabFactory.Instantiate(userId, tickId, prefab, position, rotation, parent)) : (worldPositionStays ? PrefabFactory.Instantiate(prefab, parent, worldPositionStays) : PrefabFactory.Instantiate(prefab, position, rotation, parent));
        var eb = go.GetComponent<EntityBehaviour>() ?? go.AddComponent<EntityBehaviour>();
        Despawn(eb.Entity, true);
    }

    public IEntity Spawn(int tickId = -1)
    {
        if (inactiveParts.Count > 0)
        {
            var entity = inactiveParts[0];
            var viewComponent = entity.GetComponent<ViewComponent>();
            if (entity.HasComponent<NetworkIdentityComponent>())
            {
                var networkIdentityComponent = entity.GetComponent<NetworkIdentityComponent>();
                networkIdentityComponent.TickIdWhenCreated = tickId;
            }
            inactiveParts.RemoveAt(0);
            activeParts.Add(entity);
            viewComponent.Transforms[0].gameObject.SetActive(true);
            return entity;
        }
        return null;
    }

    private void Despawn(IEntity entity, bool unlimited)
    {
        var viewComponent = entity.GetComponent<ViewComponent>();
        if (entity.HasComponent<NetworkIdentityComponent>())
        {
            var networkIdentityComponent = entity.GetComponent<NetworkIdentityComponent>();
            networkIdentityComponent.TickIdWhenCreated = -1;
        }
        if (unlimited || activeParts.Contains(entity))
        {
            viewComponent.Transforms[0].gameObject.SetActive(false);
            inactiveParts.Add(entity);
        }
        if (activeParts.Contains(entity))
        {
            activeParts.Remove(entity);
        }
    }

    public void Despawn(IEntity entity)
    {
        Despawn(entity, false);
    }

    public void Destroy(bool force)
    {
        while (inactiveParts.Count > 0)
        {
            var viewComponent = inactiveParts[0].GetComponent<ViewComponent>();
            Destroy(viewComponent.Transforms[0].gameObject);
            inactiveParts.RemoveAt(0);
        }
        if (force)
        {
            while (activeParts.Count > 0)
            {
                var viewComponent = activeParts[0].GetComponent<ViewComponent>();
                Destroy(viewComponent.Transforms[0].gameObject);
                activeParts.RemoveAt(0);
            }
        }
    }
}
