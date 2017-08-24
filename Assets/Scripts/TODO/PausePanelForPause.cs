using UnityEngine;
using UniECS;

public class PausePanelForPause : ComponentBehaviour
{
	public GameObject pausePanel;
	public CanvasGroup canvasGroup;

//	void Start ()
//	{
//		canvasGroup = pausePanel.GetComponent<CanvasGroup> ();
//	}
//
//	void Update ()
//	{
//		if (Time.timeScale==0) {
//			pausePanel.SetActive (true);
//			canvasGroup.alpha = 1;
//		} else {
//			pausePanel.SetActive (false);
//			canvasGroup.alpha = 0;
//		}
//	}
}
