using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniECS;

[System.Serializable]
public class AxleInfo
{
	public WheelCollider leftWheel;
	public WheelCollider rightWheel;
	public bool motor;
	public bool steering;

}

public class CarWheelCollider : ComponentBehaviour
{
	public List<AxleInfo> axleInfos;
//	public float maxMotorTorque;
	public float maxSteeringAngle;
	[Range (0, 99)]
	public float Speed = 50;
	public Transform target;
}
