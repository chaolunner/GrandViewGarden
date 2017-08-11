using UnityEngine;

public class WheelRun : MonoBehaviour {



	void Start () {

	}
		
	void Update () {
		transform.Rotate(new Vector3(0,-1,0) *10,Space.Self);
	}
}
