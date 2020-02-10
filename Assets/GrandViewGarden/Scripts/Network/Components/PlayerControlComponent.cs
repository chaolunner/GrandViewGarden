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

    private List<FixVector3> velocityList = new List<FixVector3>();
    private const int MaxVelocityCount = 5;

    public FixVector3 Velocity
    {
        get
        {
            var sum = FixVector3.zero;
            for (int i = 0; i < velocityList.Count; i++)
            {
                sum += velocityList[i];
            }
            return new FixVector3(sum.x / velocityList.Count, sum.y / velocityList.Count, sum.z / velocityList.Count);
        }
        set
        {
            velocityList.Add(value);
            while (velocityList.Count > MaxVelocityCount)
            {
                velocityList.RemoveAt(0);
            }
        }
    }
}
