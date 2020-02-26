using UnityEngine;
using UniEasy.ECS;
using UniEasy;

public class ThridPersonCameraComponent : ComponentBehaviour
{
    [Reorderable]
    public Vector3[] FollowOffset;

    [HideInInspector] public float smoothTime;
}
