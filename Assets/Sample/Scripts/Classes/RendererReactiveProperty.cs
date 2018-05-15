using UnityEngine;
using System;
using UniRx;

[Serializable]
public class RendererReactiveProperty : ReactiveProperty<Renderer>
{
	public RendererReactiveProperty ()
	{
	}

	public RendererReactiveProperty (Renderer initialValue)
		: base (initialValue)
	{
	}
}