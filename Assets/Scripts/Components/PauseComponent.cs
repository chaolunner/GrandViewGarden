using UnityEngine;
using UniECS;
using UniRx;

public class PauseComponent : ComponentBehaviour
{
	public BoolReactiveProperty IsPause = new BoolReactiveProperty ();
}
