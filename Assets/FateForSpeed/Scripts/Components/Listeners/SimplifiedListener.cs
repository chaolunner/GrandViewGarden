using System.Collections.Generic;
using UnityEngine;

public class SimplifiedListener : ListenerBehaviour<GameObject>
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
