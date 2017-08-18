using UnityEngine;
using UniECS;

public class PunchButton : ComponentBehaviour
{
	[Range (0, 2)]
	public float Scale = 1.2f;
	[Range (0, 1)]
	public float Duration = 0.2f;
}
