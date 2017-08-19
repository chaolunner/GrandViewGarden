using UniRx.Triggers;
using UnityEngine;
using DG.Tweening;
using System;
using UniECS;
using UniRx;

public class FreeDrivingSystem : SystemBehaviour
{
	public override void Awake ()
	{
		base.Awake ();

		var FreeDrivingEntities = GroupFactory.Create (new Type[] {
			typeof(FreeDriving),
			typeof(Animator),
		});

		FreeDrivingEntities.OnAdd ().Subscribe (entity => {
			var freeDriving = entity.GetComponent<FreeDriving> ();
			var animator = entity.GetComponent<Animator> ();
			Tweener directionTweener = null;
			Tweener rotationTweener = null;

			freeDriving.PoseIndex.DistinctUntilChanged ().Subscribe (index => {
				if (rotationTweener != null) {
					rotationTweener.Kill ();
				}
				if (directionTweener != null) {
					directionTweener.Kill ();
				}

				var selected = freeDriving.options [index];
				var rotation = animator.GetFloat ("Rotation");
				var direction = animator.GetFloat ("Direction");
				var duration = Mathf.Abs (selected - rotation) / (freeDriving.Speed * Time.unscaledDeltaTime);

				rotationTweener = DOTween.To (() => rotation, setter => animator.SetFloat ("Rotation", setter), selected, duration);
				rotationTweener.SetEase (Ease.InOutQuad);
				rotationTweener.SetDelay (freeDriving.Delay);
				rotationTweener.OnComplete (() => {
					freeDriving.PoseIndex.Value = UnityEngine.Random.Range (0, freeDriving.options.Length);
				});

				if (selected != rotation) {
					directionTweener = DOTween.To (() => direction, setter => animator.SetFloat ("Direction", setter), selected < rotation ? 1 : -1, 0.5f * duration);
					directionTweener.SetEase (Ease.InQuad);
					directionTweener.SetDelay (freeDriving.Delay);
					directionTweener.OnComplete (() => {
						direction = animator.GetFloat ("Direction");
						directionTweener = DOTween.To (() => direction, setter => animator.SetFloat ("Direction", setter), 0, 0.5f * duration);
						directionTweener.SetEase (Ease.OutQuad);
					});
				}
			}).AddTo (this.Disposer).AddTo (freeDriving.Disposer);
		}).AddTo (this.Disposer);
	}
}
