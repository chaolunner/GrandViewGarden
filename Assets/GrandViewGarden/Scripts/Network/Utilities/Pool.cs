using System.Collections.Generic;
using UnityEngine;
using UniEasy.ECS;

public interface IPool
{
    IEnumerable<IEntity> Entities { get; }
    GameObject Prefab { get; }
    void Create(Transform parent, bool worldPositionStays);
    void Create(int userId, bool worldPositionStays);
    IEntity Pop(int tickId);
    void Push(IEntity entity);
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

    public void Create(Transform parent, bool worldPositionStays)
    {
        var go = PrefabFactory.Instantiate(prefab, parent, worldPositionStays);
        var eb = go.GetComponent<EntityBehaviour>() ?? go.AddComponent<EntityBehaviour>();
        Push(eb.Entity, true);
    }

    public void Create(int userId, bool worldPositionStays)
    {
        var go = NetworkPrefabFactory.Instantiate(userId, -1, prefab, worldPositionStays);
        var eb = go.GetComponent<EntityBehaviour>() ?? go.AddComponent<EntityBehaviour>();
        Push(eb.Entity, true);
    }

    public IEntity Pop(int tickId)
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

    private void Push(IEntity entity, bool unlimited)
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

    public void Push(IEntity entity)
    {
        Push(entity, false);
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
