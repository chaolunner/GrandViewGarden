using UnityEngine;
using UniECS;
using UniRx;

public class ExchangePosition : ComponentBehaviour
{
	public BoolReactiveProperty IsOn;
	public Transform[] Origins;
	public Transform[] Targets;
	[Range (0, 1)]
	public float Duration = 0.5f;
	[Range (0, 2)]
	public float Delay = 0;
}
