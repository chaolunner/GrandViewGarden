using UnityEngine.SceneManagement;
using UnityEngine;

public class LoadSceneHistory : MonoBehaviour
{
	static public string PreviousSceneName;

	void Awake ()
	{
		SceneManager.sceneLoaded += (scene, mode) => {
			PreviousSceneName = scene.name;
			if (string.IsNullOrEmpty (PreviousSceneName)) {
				PreviousSceneName = SceneManager.GetActiveScene ().name;
			}
		};
	}
}
