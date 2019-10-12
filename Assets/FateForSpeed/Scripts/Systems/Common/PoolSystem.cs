using System.Collections.Generic;
using UnityEngine;
using UniEasy.ECS;

public class PoolSystem : SystemBehaviour, IPoolSystem
{
    public Dictionary<GameObject, List<GameObject>> ObjectsPool = new Dictionary<GameObject, List<GameObject>>();
    public Dictionary<GameObject, GameObject> PrefabsIndex = new Dictionary<GameObject, GameObject>();

    public void Alloc(GameObject prefab, Transform parent, int count)
    {
        if (ObjectsPool.ContainsKey(prefab))
        {
            int index = 0;
            while (ObjectsPool[prefab].Count > index)
            {
                if (ObjectsPool[prefab][index] == null)
                {
                    ObjectsPool[prefab].RemoveAt(index);
                }
                else
                {
                    index++;
                }
            }
        }
        else
        {
            ObjectsPool.Add(prefab, new List<GameObject>());
        }

        while (ObjectsPool[prefab].Count < count)
        {
            Recycle(prefab, PrefabFactory.Instantiate(prefab, parent), parent);
        }

        while (ObjectsPool[prefab].Count > count)
        {
            Destroy(ObjectsPool[prefab][0]);
            ObjectsPool[prefab].RemoveAt(0);
        }
    }

    public GameObject Alloc(GameObject prefab, Transform parent)
    {
        Alloc(prefab, parent, 1);

        var go = ObjectsPool[prefab][0];
        if (go)
        {
            ObjectsPool[prefab].RemoveAt(0);
            go.SetActive(true);
        }
        return go;
    }

    public void Recycle(GameObject prefab, GameObject go, Transform parent)
    {
        if (!ObjectsPool.ContainsKey(prefab))
        {
            ObjectsPool.Add(prefab, new List<GameObject>());
        }

        if (!ObjectsPool[prefab].Contains(go))
        {
            ObjectsPool[prefab].Add(go);
        }

        go.transform.SetParent(parent);
        go.SetActive(false);
    }
}
