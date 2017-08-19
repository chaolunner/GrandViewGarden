using UnityEngine.SceneManagement;
using UniRx.Triggers;
using UnityEngine;
using UniRx;

public class GoBackView : MonoBehaviour
{
	public string sceneName = "Overview";

	void Start ()
	{
		Time.timeScale = 1;
		gameObject.OnPointerClickAsObservable ().Subscribe (_ => {
			SceneManager.LoadScene (sceneName);
		});
//		GameObject view = GameObject.Find("Level");        
//		GameObject car = view.transform.Find("OverviewCar").gameObject;       
//		car.SetActive(true);
	}
}
