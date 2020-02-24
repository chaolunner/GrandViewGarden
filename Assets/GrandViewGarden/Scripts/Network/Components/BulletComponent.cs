using UnityEngine;
using UniEasy.ECS;
using Common;

public class BulletComponent : ComponentBehaviour
{
    public Fix64 radius;
    public FixVector3 velocity;

    [HideInInspector] public float holeSize = 1;
}
