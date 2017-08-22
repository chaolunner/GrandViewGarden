using UnityEngine;
using UniECS;
using UniRx;

public class Timing : ComponentBehaviour
{
	public  int Hour;
	public int Minute;
	public int Second;
	public int MilliSecond;
	public float TimeSpeed = 0.0f;
}
