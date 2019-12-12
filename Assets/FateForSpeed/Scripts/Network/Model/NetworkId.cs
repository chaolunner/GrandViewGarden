using System;

[Serializable]
public struct NetworkId
{
    public int UserId;
    public int InstanceId;

    public NetworkId(int userId, int instanceId)
    {
        UserId = userId;
        InstanceId = instanceId;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = 17;

            hashCode = (hashCode * 23) + UserId;

            hashCode = (hashCode * 23) + InstanceId;

            return hashCode;
        }
    }

    public override bool Equals(object obj)
    {
        return obj is NetworkId && ((NetworkId)obj) == this;
    }

    public static bool operator ==(NetworkId lhs, NetworkId rhs)
    {
        return lhs.UserId == rhs.UserId && lhs.InstanceId == rhs.InstanceId;
    }

    public static bool operator !=(NetworkId lhs, NetworkId rhs)
    {
        return lhs.UserId != rhs.UserId || lhs.InstanceId != rhs.InstanceId;
    }
}
