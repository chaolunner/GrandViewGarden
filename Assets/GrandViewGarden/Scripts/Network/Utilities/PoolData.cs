using UnityEngine;

public struct PoolData
{
    public GameObject Prefab;
    public int UserId;
    public Transform Parent;
    public bool WorldPositionStays;

    public PoolData(GameObject prefab, int userId, bool worldPositionStays = false)
    {
        Prefab = prefab;
        UserId = userId;
        Parent = null;
        WorldPositionStays = worldPositionStays;
    }

    public PoolData(GameObject prefab, Transform parent, bool worldPositionStays = false)
    {
        Prefab = prefab;
        UserId = -1;
        Parent = parent;
        WorldPositionStays = worldPositionStays;
    }
}
