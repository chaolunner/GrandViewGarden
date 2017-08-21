using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniECS;

[System.Serializable]
public class AxleInfo{
	public WheelCollider leftWheel;
	public WheelCollider rightWheel;
	public bool motor;
	public bool steering;

}
public class CarWheelCollider : ComponentBehaviour {
	public List<AxleInfo> axleInfos;
	public float maxMotorTorque;
	public float maxSteeringAngle;
	[Range (0,99)]
	public float speed=50;

//	public void ApplyLocalPositionToVisuals(WheelCollider collider){
//		if (collider.transform.childCount == 0) {
//			return;
//		}
//		Transform visualWheel = collider.transform.GetChild (0);
//
//		Vector3 position;
//		Quaternion rotation;
//		collider.GetWorldPose (out position, out rotation);
//
//		visualWheel.transform.position = position;
//		visualWheel.transform.rotation = rotation;
//	}
//
//	void FixedUpdate(){		
//
//		float motor = speed+ maxMotorTorque *Input.GetAxis("Vertical");
//		float steering = maxSteeringAngle * Input.GetAxis ("Horizontal");
//
//		foreach (AxleInfo axleInfo in axleInfos) {
//			if (axleInfo.steering) {
//				axleInfo.leftWheel.steerAngle = steering;
//				axleInfo.rightWheel.steerAngle = steering;
//			}
//			if (axleInfo.motor) {
//				axleInfo.leftWheel.motorTorque = motor;
//				axleInfo.rightWheel.motorTorque = motor;
//			}
//			ApplyLocalPositionToVisuals (axleInfo.leftWheel);
//			ApplyLocalPositionToVisuals (axleInfo.rightWheel);
//		}
//	}
}
