using System.Collections.Generic;
using UnityEngine;
using UniEasy.ECS;

public interface IPool
{
    IEnumerable<GameObject> Elements { get; }
    GameObject Prefab { get; }
    void Create(Transform parent, bool worldPositionStays);
    void Create(int userId, bool worldPositionStays);
    GameObject Pop();
    void Push(GameObject go);
    /// <param name="force">Whether to destroy unrecycled game objects.</param>
    void Destroy(bool force);
}

public class Pool : IPool
{
    private GameObject prefab;
    private List<GameObject> elements;
    private List<GameObject> inactiveParts;
    private List<GameObject> activeParts;
    private PrefabFactory PrefabFactory;
    private NetworkPrefabFactory NetworkPrefabFactory;

    public IEnumerable<GameObject> Elements
    {
        get
        {
            elements.Clear();
            elements.AddRange(inactiveParts);
            elements.AddRange(activeParts);
            return elements;
        }
    }

    public GameObject Prefab
    {
        get { return prefab; }
    }

    public Pool(GameObject prefab, PrefabFactory prefabFactory, NetworkPrefabFactory networkPrefabFactory)
    {
        inactiveParts = new List<GameObject>();
        activeParts = new List<GameObject>();
        this.prefab = prefab;
        PrefabFactory = prefabFactory;
        NetworkPrefabFactory = networkPrefabFactory;
    }

    public void Create(Transform parent, bool worldPositionStays)
    {
        Push(PrefabFactory.Instantiate(prefab, parent, worldPositionStays), true);
    }

    public void Create(int userId, bool worldPositionStays)
    {
        Push(NetworkPrefabFactory.Instantiate(userId, prefab, worldPositionStays), true);
    }

    public GameObject Pop()
    {
        if (inactiveParts.Count > 0)
        {
            var go = inactiveParts[0];
            inactiveParts.RemoveAt(0);
            activeParts.Add(go);
            go.SetActive(true);
            return go;
        }
        return null;
    }

    private void Push(GameObject go, bool unlimited)
    {
        if (activeParts.Contains(go))
        {
            go.SetActive(false);
            inactiveParts.Add(go);
            activeParts.Remove(go);
        }
        else if (unlimited)
        {
            go.SetActive(false);
            inactiveParts.Add(go);
        }
    }

    public void Push(GameObject go)
    {
        Push(go, false);
    }

    public void Destroy(bool force)
    {
        while (inactiveParts.Count > 0)
        {
            Destroy(inactiveParts[0]);
            inactiveParts.RemoveAt(0);
        }
        if (force)
        {
            while (activeParts.Count > 0)
            {
                Destroy(activeParts[0]);
                activeParts.RemoveAt(0);
            }
        }
    }
}
