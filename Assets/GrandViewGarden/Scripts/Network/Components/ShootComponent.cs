using System.Collections.Generic;
using UnityEngine;
using UniEasy.ECS;
using UniEasy;
using Common;
using UniRx;

public class ShootComponent : ComponentBehaviour
{
    public Transform Parent;
    [Range(0, 90)]
    public float LimitAngle = 25;
    public IntReactiveProperty WeaponIndex;
    [Reorderable]
    public List<string> Weapons;
    public IEntity CurrentWeaponEntity;

    [HideInInspector] public GameObject bulletPrefab;
    [HideInInspector] public GameObject muzzleFlashesPrefab;
    [HideInInspector] public Vector3 adsPosition;
    [HideInInspector] public Vector3 bulletLocalPosition;
    [HideInInspector] public Vector3 muzzleFlashesPosition;
    [HideInInspector] public Quaternion weaponLocalRotation;
    [HideInInspector] public float holeSize;
    [HideInInspector] public float speed;
    [HideInInspector] public float cooldown;
    [HideInInspector] public Fix64 cooldownTime;
}
