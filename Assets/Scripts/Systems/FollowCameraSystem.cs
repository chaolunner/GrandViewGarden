using UniRx.Triggers;
using System.Collections;
using System;
using UnityEngine;
using UniECS;
using UniRx;


public class FollowCameraSystem :SystemBehaviour
{		
	public override void Awake ()
	{
		base.Awake ();

		var FollowCameraEntities = GroupFactory.Create (new Type[] {
			typeof(FollowCamera)	
		});

		FollowCameraEntities.OnAdd ().Subscribe (entitiy => {
			var followCamera = entitiy.GetComponent<FollowCamera>();
			followCamera.offset = followCamera.transform.position - followCamera.Car.position;

			Observable.EveryUpdate().Subscribe(_ => {		
				Vector3 targetCameraPosition = followCamera.Car.position + followCamera.offset;
				followCamera.transform.position = Vector3.Lerp (followCamera.transform.position, targetCameraPosition, followCamera.speed * Time.deltaTime);
			}).AddTo (this.Disposer).AddTo (followCamera.Disposer);
		}).AddTo (this.Disposer);
	}
}
