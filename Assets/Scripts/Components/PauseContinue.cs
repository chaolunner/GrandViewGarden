using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UniECS;

public class PauseContinue : ComponentBehaviour
{
	public RectTransform gamePanel;
	public RectTransform pausePanel;
	public Button contBtn;
	public Button pauseBtn;


//	void Start ()
//	{
//		var pausepos = pausePanel.position;
//		var gamePos = gamePanel.position;
//		pauseBtn.onClick.AddListener (delegate() {
//			gamePanel.position = pausepos;
//			pausePanel.position = gamePos;
//			pause = true;
//	
//		});
//		contBtn.onClick.AddListener (delegate() {
//			gamePanel.position = gamePos;
//			pausePanel.position = pausepos;
//			pause = false;
//		});
//	}
//
//	void Update ()
//	{
//		if (pause) {
//			Time.timeScale = 0;			
//		} else {
//			Time.timeScale = 1;	
//		}
//	}
	
}
