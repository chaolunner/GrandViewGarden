using UniRx.Triggers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniECS;
using System;
using UniRx;

public class SpeedSystem : SystemBehaviour
{
	public override void Awake ()
	{
		base.Awake ();
		var SpeedEntities = GroupFactory.Create (new Type[] {
			typeof(Speed),
			typeof(CarWheelCollider)	
		});
		SpeedEntities.OnAdd ().Subscribe (entity => {
			var speed = entity.GetComponent<Speed> ();
			var carWheelCollider = entity.GetComponent<CarWheelCollider> ();
			speed.OnTriggerEnterAsObservable ().Subscribe (data => {
				if (data.tag == "speedObject") {
					carWheelCollider.Speed = carWheelCollider.Speed * 2;
				}				
			}).AddTo (this.Disposer).AddTo (speed.Disposer);
		}).AddTo (this.Disposer);
	}
}
