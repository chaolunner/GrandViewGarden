using UnityEngine;
using UniECS;
using UniRx;

public class CenterOnChild : ComponentBehaviour
{
	[Range (0, 20)]
	public float CenterSpeed = 5f;
	public TransformReactiveProperty Target;
}
