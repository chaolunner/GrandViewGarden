using UnityEngine;
using UniECS;

public enum FadeInType
{
	Panel,
	Scene,
}

public class FadeInTag : ComponentBehaviour
{
	public FadeInType FadeInType;
}
