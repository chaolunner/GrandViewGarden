using System.Collections.Generic;
using UnityEngine;
using UniEasy.ECS;
using UniEasy;
using UniRx;

public class ShootComponent : ComponentBehaviour
{
    public Transform Parent;
    public IntReactiveProperty WeaponIndex;
    [Reorderable]
    public List<string> Weapons;

    [HideInInspector] public GameObject weapon;
    [HideInInspector] public Vector3 adsPosition;
}
