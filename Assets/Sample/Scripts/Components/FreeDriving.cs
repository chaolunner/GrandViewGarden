using UnityEngine;
using UniECS;
using UniRx;

public class FreeDriving : ComponentBehaviour
{
	[Range (1, 30)]
	public float Speed = 10;
	[Range (0, 10)]
	public float Delay = 1;
	[HideInInspector]
	public IntReactiveProperty PoseIndex;
	public readonly float[] options = new float [] { -1, -0.25f, 0.5f };
}
