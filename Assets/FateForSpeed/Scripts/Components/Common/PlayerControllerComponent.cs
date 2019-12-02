﻿using UnityEngine;
using UniEasy.ECS;

public class PlayerControllerComponent : ComponentBehaviour
{
    public float Speed = 6;
    public float JumpSpeed = 8;
    public float Gravity = 20;

    [HideInInspector]
    public Vector3 Motion;
}
