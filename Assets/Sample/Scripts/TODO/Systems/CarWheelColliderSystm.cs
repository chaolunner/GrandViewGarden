using UniRx.Triggers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniECS;
using System;
using UniRx;

public class CarWheelColliderSystm : SystemBehaviour
{
	public override void Awake ()
	{
		base.Awake ();

		var CarWheelColliderEntities = GroupFactory.Create (new Type[] {
			typeof(CarWheelCollider)	
		});

		CarWheelColliderEntities.OnAdd ().Subscribe (entity => {
			var carWheelCollider = entity.GetComponent<CarWheelCollider> ();

			Observable.EveryFixedUpdate ().Subscribe (_ => {
				float motor = carWheelCollider.Speed * 0.3f;
//				float steering = carWheelCollider.maxSteeringAngle * Input.GetAxis ("Horizontal");
				carWheelCollider.transform.position = Vector3.MoveTowards (carWheelCollider.transform.position, carWheelCollider.target.position, carWheelCollider.Speed * 0.7f * Time.deltaTime);

				foreach (AxleInfo axleInfo in carWheelCollider.axleInfos) {
//					if (axleInfo.steering) {
//						axleInfo.leftWheel.steerAngle = steering;
//						axleInfo.rightWheel.steerAngle = steering;
//					}
					if (axleInfo.motor) {
						axleInfo.leftWheel.motorTorque = motor;
						axleInfo.rightWheel.motorTorque = motor;
					}
					ApplyLocalPositionToVisuals (axleInfo.leftWheel);
					ApplyLocalPositionToVisuals (axleInfo.rightWheel);
				}	
			}).AddTo (this.Disposer).AddTo (carWheelCollider.Disposer);
		}).AddTo (this.Disposer);
	}

	public void ApplyLocalPositionToVisuals (WheelCollider collider)
	{
		if (collider.transform.childCount == 0) {
			return;
		}
		Transform visualWheel = collider.transform.GetChild (0);

		Vector3 position;
		Quaternion rotation;
		collider.GetWorldPose (out position, out rotation);

		visualWheel.transform.position = position;
		visualWheel.transform.rotation = rotation;
	}
}
