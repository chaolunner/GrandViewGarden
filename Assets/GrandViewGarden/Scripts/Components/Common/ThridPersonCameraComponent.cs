using UnityEngine;
using UniEasy.ECS;
using UniEasy;

public class ThridPersonCameraComponent : ComponentBehaviour
{
    public float Smooth = 4;
    [Reorderable]
    public Vector3[] FollowOffset;

    [HideInInspector] public float smoothTime;
}
