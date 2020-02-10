using UnityEngine;

public struct PlayerControlResult : IUserInputResult
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Quaternion Follow;
}
