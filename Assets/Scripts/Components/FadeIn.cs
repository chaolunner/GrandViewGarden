using UnityEngine;
using UniECS;
using UniRx;

public enum FadeInType
{
	None,
	Panel,
	Scene,
}

public class FadeIn : ComponentBehaviour
{
	public FadeInType FadeInType;
	[RangeReactiveProperty (0, 1)]
	public FloatReactiveProperty NormalizedTime;
	[Range (0, 5)]
	public float Duration = 0.3f;
	public FadeInSequence Sequence;
}
