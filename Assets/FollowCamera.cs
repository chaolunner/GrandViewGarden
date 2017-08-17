using System.Collections;
using UnityEngine;

public class FollowCamera : MonoBehaviour {
	public Transform Car;
	public float speed = 5f;
	Vector3 offset;

	void Start () {		
		offset = transform.position-Car.position  ;
	}

	void Update () {
		Vector3 targetCameraPosition = Car.position + offset;
		transform.position = Vector3.Lerp (transform.position, targetCameraPosition, speed * Time.deltaTime);
	}
}
