using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseContinue : MonoBehaviour {
	public RectTransform gamePanel;
	public RectTransform pausePanel;
	public Button contBtn;
	public Button pauseBtn;
	private Vector3 pausepos;
	private Vector3 gamePos;
	bool pause =false;

	void Start () {
		pausepos = pausePanel.position;
		gamePos = gamePanel.position;
		pauseBtn.onClick.AddListener(delegate() 
			{
				gamePanel.position = pausepos;
				pausePanel.position = gamePos;
				pause =true;

			});
		contBtn.onClick.AddListener(delegate() 
			{
				gamePanel.position = gamePos;
				pausePanel.position = pausepos;
				pause =false;
			});
	}

	void Update(){
		if (pause) {
//			Time.timeScale = 0;
		}else {
			Time.timeScale = 1;
		}
	}

}
