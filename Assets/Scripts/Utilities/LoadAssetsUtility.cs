using UnityEngine;

public class LoadAssetsUtility : MonoBehaviour
{
	[RuntimeInitializeOnLoadMethod]
	static public void Steup ()
	{
		foreach (var o in Resources.LoadAll("Kernel")) {
			DontDestroyOnLoad (Instantiate<Object> (o));
		}
	}
}
