using UnityEngine;
using UniEasy.ECS;
using UniEasy;
using UniRx;

public class FootstepsComponent : ComponentBehaviour
{
    [Reorderable]
    public Transform[] Foots;
    public ReactiveCollection<int> Footsteps = new ReactiveCollection<int>();

    public void Footstep(int index)
    {
        if (!Footsteps.Contains(index))
        {
            Footsteps.Add(index);
        }
    }
}
