using UnityEngine;
using System;
using UniRx;

[Serializable]
public class GameObjectReactiveProperty : ReactiveProperty<GameObject>
{
	public GameObjectReactiveProperty ()
	{
	}

	public GameObjectReactiveProperty (GameObject initialValue)
		: base (initialValue)
	{
	}
}
