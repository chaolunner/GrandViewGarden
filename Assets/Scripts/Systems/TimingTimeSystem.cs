using UniRx.Triggers;
using System.Collections;
using UniECS;
using UnityEngine.UI;
using UnityEngine;
using System;
using UniRx;

public class TimingTimeSystem : SystemBehaviour
{
	public override void Awake ()
	{
		base.Awake ();

		var TimingEntities = GroupFactory.Create (new Type[] {
			typeof(TimingTime)
		});

		TimingEntities.OnAdd ().Subscribe (entitiy => {
			var timingTime = entitiy.GetComponent<TimingTime> ();

			Observable.EveryUpdate().Subscribe(_ => {		
				timingTime.timeSpeed += Time.deltaTime;	

				var hour = (int) timingTime.timeSpeed / 3600;
				var minute = ((int)timingTime.timeSpeed - hour * 3600) / 60;
				var second = (int)timingTime.timeSpeed - hour * 3600 - minute * 60;
				var millisecond = (int)((timingTime.timeSpeed - (int)timingTime.timeSpeed) * 100);

				timingTime.text_timeSpeed.text = string.Format ("{0:D2}.{1:D2}", second,millisecond);
			}).AddTo (this.Disposer).AddTo (timingTime.Disposer);
		}).AddTo (this.Disposer);
	}
}
