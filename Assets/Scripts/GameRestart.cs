using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRestart : MonoBehaviour {
//	public Scene scene;

	void Start () {
//		scene = SceneManager.GetActiveScene ().name;
	}
	

	void Update () {
		if (transform.position.y <= -10) {
			Application.LoadLevel(Application.loadedLevel);//restart game
		}
	}
}
