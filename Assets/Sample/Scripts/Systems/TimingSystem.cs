using UnityEngine.UI;
using UnityEngine;
using UniECS;
using System;
using UniRx;

public class TimingSystem : SystemBehaviour
{
	public override void Awake ()
	{
		base.Awake ();

		var TimingEntities = GroupFactory.Create (new Type[] {
			typeof(Timing)
		});

		TimingEntities.OnAdd ().Subscribe (entity => {
			var timing = entity.GetComponent<Timing> ();

			Observable.EveryUpdate ().Subscribe (_ => {
				timing.TimeSpeed += Time.deltaTime;

				timing.Hour = (int)timing.TimeSpeed / 3600;
				timing.Minute = ((int)timing.TimeSpeed - timing.Hour * 3600) / 60;
				timing.Second = (int)timing.TimeSpeed - timing.Hour * 3600 - timing.Minute * 60;
				timing.MilliSecond = (int)((timing.TimeSpeed - (int)timing.TimeSpeed) * 100);

				if (entity.HasComponent<Text> ()) {
					entity.GetComponent<Text> ().text = string.Format ("{0:D2}.{1:D2}", timing.Second, timing.MilliSecond);
				}
			}).AddTo (this.Disposer).AddTo (timing.Disposer);
		}).AddTo (this.Disposer);
	}
}
