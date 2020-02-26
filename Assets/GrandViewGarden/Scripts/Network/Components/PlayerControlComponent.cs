using System.Collections.Generic;
using UnityEngine;
using UniEasy.ECS;
using UniEasy;
using Common;

public class PlayerControlComponent : ComponentBehaviour
{
    public float Run = 6;
    public float Walk = 3;
    public float Jump = 8;
    public float Gravity = 20;

    public Vector2 MouseSensivity = new Vector2(200, 100);
    [MinMaxRange(-180, 180)]
    public RangedFloat YAngleLimit = new RangedFloat(-60, 60);
    public AimModeReactiveProperty Aim;
    public bool Crouched;
    public Transform Follow;
    public Transform LookAt;

    [HideInInspector] public Fix64 smoothTime;
    [HideInInspector] public Fix64 aimTime;
    [HideInInspector] public FixVector3 motion;
}
