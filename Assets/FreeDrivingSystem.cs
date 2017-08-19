using UniRx.Triggers;
using UnityEngine;
using DG.Tweening;
using UniECS;
using System;
using UniRx;


public class FreeDrivingSystem : SystemBehaviour
{
	public override void Awake ()
	{
		base.Awake ();
		var freeDrivingEntities = GroupFactory.Create (new Type[] {
			typeof(FreeDriving)
		});

		freeDrivingEntities.OnAdd ().Subscribe (entity => {
			var freeDriving = entity.GetComponent<FreeDriving> ();
			freeDriving.animator = GetComponent<Animator> ();

			freeDriving.OnEnableAsObservable ().Subscribe (_ => {
				var index = UnityEngine.Random.Range (0, freeDriving.options.Length);
				var selected = freeDriving.options [index];
				var rotation = freeDriving.animator.GetFloat ("Rotation");
				var direction = freeDriving.animator.GetFloat ("Direction");
				var duration = Mathf.Abs (selected - rotation) / (freeDriving.Speed * Time.unscaledDeltaTime);

				freeDriving.rotationTweener = DOTween.To (() => rotation, setter => freeDriving.animator.SetFloat ("Rotation", setter), selected, duration);
				freeDriving.rotationTweener.SetEase (Ease.InOutQuad);
				freeDriving.rotationTweener.SetDelay (freeDriving.Delay);
				freeDriving.rotationTweener.OnComplete (() => {
					freeDriving.OnDisableAsObservable ();
					freeDriving.OnEnableAsObservable ();
				});

				if (selected != rotation) {
					freeDriving.directionTweener = DOTween.To (() => direction, setter => freeDriving.animator.SetFloat ("Direction", setter), selected < rotation ? 1 : -1, 0.5f * duration);
					freeDriving.directionTweener.SetEase (Ease.InQuad);
					freeDriving.directionTweener.SetDelay (freeDriving.Delay);
					freeDriving.directionTweener.OnComplete (() => {
						direction = freeDriving.animator.GetFloat ("Direction");
						freeDriving.directionTweener = DOTween.To (() => direction, setter => freeDriving.animator.SetFloat ("Direction", setter), 0, 0.5f * duration);
						freeDriving.directionTweener.SetEase (Ease.OutQuad);
					});
				}

			});

			freeDriving.OnDisableAsObservable ().Subscribe (_=>{
				if (freeDriving.rotationTweener != null) {
					freeDriving.rotationTweener.Kill ();
				}
				if (freeDriving.directionTweener != null) {
					freeDriving.directionTweener.Kill ();
				}

			});

		});
	}

}
