using System.Collections.Generic;
using UnityEngine;
using UniEasy.ECS;
using UniEasy;
using Common;
using UniRx;

public class ShootComponent : ComponentBehaviour
{
    public Transform Parent;
    public IntReactiveProperty WeaponIndex;
    [Reorderable]
    public List<string> Weapons;
    public IEntity CurrentWeaponEntity;

    [HideInInspector] public GameObject bulletPrefab;
    [HideInInspector] public GameObject muzzleFlashesPrefab;
    [HideInInspector] public Vector3 adsPosition;
    [HideInInspector] public Vector3 bulletPosition;
    [HideInInspector] public Vector3 muzzlePosition;
    [HideInInspector] public float holeSize;
    [HideInInspector] public float speed;
    [HideInInspector] public float cooldown;
    [HideInInspector] public Fix64 cooldownTime;
}
