using UnityEngine;
using UniECS;

public class Timing : ComponentBehaviour
{
	public int Hour;
	public int Minute;
	public int Second;
	public int MilliSecond;
	[HideInInspector]
	public float TimeSpeed = 0.0f;
}
