using UnityEngine;
using System;
using UniRx;

[Serializable]
public class TransformReactiveProperty : ReactiveProperty<Transform>
{
	public TransformReactiveProperty ()
	{
	}

	public TransformReactiveProperty (Transform initialValue)
		: base (initialValue)
	{
	}
}
