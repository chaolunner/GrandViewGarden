using UniEasy.ECS;
using UnityEngine;

public class SetProgressByDistance : ComponentBehaviour
{
    public Space Space = Space.Self;
    public float Distance = 1;
    public AnimationCurve ProgressCurve = AnimationCurve.Linear(0, 0, 1, 1);
}
