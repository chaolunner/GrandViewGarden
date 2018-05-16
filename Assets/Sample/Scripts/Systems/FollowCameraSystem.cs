using UnityEngine;
using System.Linq;
using System;
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
			var followCamera = entitiy.GetComponent<FollowCamera> ();
			var offset = followCamera.transform.position - followCamera.Target.position;
			var min = followCamera.Speed.keys.Select (k => k.time).Min ();
			var max = followCamera.Speed.keys.Select (k => k.time).Max ();

			Observable.EveryUpdate ().TakeWhile (_ => followCamera.Target != null).Subscribe (_ => {		
				var distance = Vector3.Distance (followCamera.transform.position, followCamera.Target.position + offset);
				var speed = followCamera.Speed.Evaluate (Mathf.Clamp (distance, min, max));

				if (distance > max) {
					followCamera.transform.position = followCamera.Target.position + offset;
					return;
				}
				followCamera.transform.position = Vector3.Lerp (followCamera.transform.position, followCamera.Target.position + offset, speed * Time.deltaTime);
			}).AddTo (this.Disposer).AddTo (followCamera.Disposer);
		}).AddTo (this.Disposer);
	}
}
