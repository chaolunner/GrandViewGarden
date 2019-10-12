using System.Collections.Generic;
using UnityEngine;

public class GameObjectListener : ListenerBehaviour<GameObject, GameObject>
{
    public override List<GameObject> Targets
    {
        get
        {
            if (!targets.Contains(gameObject))
            {
                targets.Add(gameObject);
            }
            return targets;
        }
        set
        { targets = value; }
    }
}
