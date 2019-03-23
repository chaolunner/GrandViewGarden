using UniEasy.ECS;
using UnityEngine;
using UniRx;

public class CenterOnChild : ComponentBehaviour
{
    public Transform Content;
    public float Speed = 1;
    public AnimationCurve SpeedCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    public float Inertia = 1;
    public int Space = 0;
    public IntReactiveProperty Index;
}
