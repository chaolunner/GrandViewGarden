using UniEasy.ECS;
using UnityEngine;
using UniRx;

[AddComponentMenu("FSM/Toggle")]
public class FSMToggle : ComponentBehaviour
{
    public BoolReactiveProperty IsOn;
    [HideInInspector] public FSMToggleGroup Group;
}