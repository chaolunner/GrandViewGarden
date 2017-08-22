using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speed : MonoBehaviour {
	public CarWheelCollider carWheelCollider;
	void Start () {
		var carWheelCollider = GetComponent<CarWheelCollider> ();
	}
	

	void OnTriggerEnter(Collider collider)
	{
		Debug.Log (collider.gameObject.name);
		if (collider.gameObject.name == "sphere") {
			carWheelCollider.maxMotorTorque  = carWheelCollider.maxMotorTorque + 100f;

		}
	}
		

}
