using UnityEngine;

public interface IPoolSystem
{
    void Alloc(GameObject prefab, Transform parent, int count = 1);
    GameObject Alloc(GameObject prefab, Transform parent);
    void Recycle(GameObject prefab, GameObject go, Transform parent);
}
