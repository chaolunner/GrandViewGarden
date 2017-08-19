using UnityEngine;
using UniECS;

public class FadeInScreen : ComponentBehaviour
{
	public CanvasGroup Mask;
	[Range (0, 1)]
	public float Alpha = 1;
	[Range (0, 1)]
	public float Duration = 0.2f;
	[Range (0, 1)]
	public float FadeInDelay = 0;
	[Range (0, 1)]
	public float FadeOutDelay = 0;
}

