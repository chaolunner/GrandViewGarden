using UnityEngine.UI;
using UnityEngine;
using UniECS;

public class FadeInCenter : ComponentBehaviour
{
	[Range (0, 3)]
	public float Scale = 1.5f;
	[MinMaxRange (0, 1)]
	public RangedFloat AlphaRange = new RangedFloat (0, 1);
}
