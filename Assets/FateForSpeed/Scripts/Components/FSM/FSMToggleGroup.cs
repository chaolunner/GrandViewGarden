using UniEasy.ECS;
using UnityEngine;
using UniRx;

[AddComponentMenu("FSM/ToggleGroup")]
public class FSMToggleGroup : ComponentBehaviour
{
    public ReactiveCollection<IEntity> Toggles = new ReactiveCollection<IEntity>();
}