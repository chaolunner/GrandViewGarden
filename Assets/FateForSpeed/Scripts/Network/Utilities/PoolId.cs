using UnityEngine;

public struct PoolId
{
    public GameObject Prefab;
    public int UserId;
    public Transform Parent;
    public bool WorldPositionStays;

    public PoolId(GameObject prefab, int userId, bool worldPositionStays = false)
    {
        Prefab = prefab;
        UserId = userId;
        Parent = null;
        WorldPositionStays = worldPositionStays;
    }

    public PoolId(GameObject prefab, Transform parent, bool worldPositionStays = false)
    {
        Prefab = prefab;
        UserId = -1;
        Parent = parent;
        WorldPositionStays = worldPositionStays;
    }

    public override int GetHashCode()
    {
        if (Parent == null)
        {
            return Prefab.GetHashCode() ^ UserId ^ WorldPositionStays.GetHashCode();
        }
        else
        {
            return Prefab.GetHashCode() ^ UserId ^ Parent.GetHashCode() ^ WorldPositionStays.GetHashCode();
        }
    }

    public override bool Equals(object obj)
    {
        return obj is PoolId && ((PoolId)obj) == this;
    }

    public static bool operator ==(PoolId lhs, PoolId rhs)
    {
        return lhs.Prefab == rhs.Prefab && lhs.UserId == rhs.UserId && lhs.Parent == rhs.Parent && lhs.WorldPositionStays == rhs.WorldPositionStays;
    }

    public static bool operator !=(PoolId lhs, PoolId rhs)
    {
        return lhs.Prefab != rhs.Prefab || lhs.UserId != rhs.UserId || lhs.Parent != rhs.Parent || lhs.WorldPositionStays != rhs.WorldPositionStays;
    }
}
