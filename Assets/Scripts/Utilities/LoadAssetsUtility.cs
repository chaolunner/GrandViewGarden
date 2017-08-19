using UnityEngine;

public class LoadAssetsUtility
{
	[RuntimeInitializeOnLoadMethod]
	static public void Steup ()
	{
		foreach (var o in Resources.LoadAll("Kernel")) {
			var go = (GameObject)GameObject.Instantiate (o);
			GameObject.DontDestroyOnLoad (go);
		}
	}
}
