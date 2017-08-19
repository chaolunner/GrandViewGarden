using UnityEngine;
using UniECS;
using UniRx;

public class FadeIn : ComponentBehaviour
{
	[RangeReactiveProperty (0, 1)]
	public FloatReactiveProperty Alpha;
	[Range (0, 1)]
	public float Duration = 0.3f;
}

