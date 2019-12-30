using System.Collections.Generic;
using UnityEngine;
using UniEasy.ECS;
using UniEasy;

public class ShootComponent : ComponentBehaviour
{
    [Reorderable]
    public List<string> Weapon;
}
