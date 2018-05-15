using UnityEngine;
using UniECS;
using UniRx;

public class ColorComponent : ComponentBehaviour
{
	public BoolReactiveProperty IncludeChild = new BoolReactiveProperty ();
	public ColorReactiveProperty Color = new ColorReactiveProperty ();
	public ReactiveCollection<ColorComponent> ColorCollection = new ReactiveCollection<ColorComponent> ();
}
