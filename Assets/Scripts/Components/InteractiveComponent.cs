using System.Collections.Generic;
using UnityEngine;
using UniECS;

public class InteractiveComponent : ComponentBehaviour
{
	public List<Transform> TouchAreas = new List<Transform> ();
}
