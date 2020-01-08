using UnityEngine;
using UniEasy.ECS;
using UniEasy;
using UniRx;

public class ThridPersonCameraComponent : ComponentBehaviour
{
    [Reorderable]
    public Vector3[] FollowOffset;

    [HideInInspector] public CompositeDisposable smoothDisposer = new CompositeDisposable();
}
