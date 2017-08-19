using UnityEngine.SceneManagement;
using UniRx.Triggers;
using UnityEngine;
using UniRx;

public class ChangeScene : MonoBehaviour
{
	public string sceneName = "Pass";

	void Start ()
	{
		Time.timeScale = 1;
		gameObject.OnPointerClickAsObservable ().Subscribe (_ => {
			SceneManager.LoadScene (sceneName);
		});
	}
}
