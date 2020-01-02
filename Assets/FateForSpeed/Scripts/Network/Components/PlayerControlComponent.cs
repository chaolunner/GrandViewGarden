using UnityEngine;
using UniEasy.ECS;
using UniEasy;

public class PlayerControlComponent : ComponentBehaviour
{
    public float Speed = 6;
    public float JumpSpeed = 8;
    public float Gravity = 20;

    public Vector2 MouseSensivity = new Vector2(200, 100);
    [MinMaxRange(-180, 180)]
    public RangedFloat YAngleLimit = new RangedFloat(-60, 60);
    public AimModeReactiveProperty Aim;
    public Transform Follow;
    public Transform LookAt;

    [HideInInspector] public float smoothTime;
    [HideInInspector] public float aimTime;
    [HideInInspector] public Vector3 motion;
}
