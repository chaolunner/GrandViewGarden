using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speed : MonoBehaviour {
	public GameObject sphere;
	public CarWheelCollider carWheelCollider;
	void Start () {
		carWheelCollider = GetComponent<CarWheelCollider> ();

	}
	

	void OnTriggerEnter(Collider collider){
		if (collider == sphere) {
			carWheelCollider.speed = carWheelCollider.speed + 100f;

		}
	}
		

}
