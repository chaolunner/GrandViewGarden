using UniRx.Triggers;
using DG.Tweening;
using UnityEngine;
using UniECS;
using System;
using UniRx;

public class FadeInScreenSystem : SystemBehaviour
{
	public override void Awake ()
	{
		base.Awake ();

		var FadeInScreenEntities = GroupFactory.Create (new Type[] {
			typeof(FadeInScreen)
		});

		FadeInScreenEntities.OnAdd ().Subscribe (entity => {
			var fadeInScreen = entity.GetComponent<FadeInScreen> ();

			fadeInScreen.OnPointerClickAsObservable ().Subscribe (_ => {
				if (fadeInScreen.Mask != null) {
					var originAlpha = fadeInScreen.Mask.alpha;
					var fadeIn = fadeInScreen.Mask.DOFade (fadeInScreen.Alpha, fadeInScreen.Duration);
					fadeIn.SetDelay (fadeInScreen.FadeInDelay);
					fadeIn.SetEase (Ease.InQuart);
					fadeIn.OnComplete (() => {
						var fadeOut = fadeInScreen.Mask.DOFade (originAlpha, fadeInScreen.Duration);
						fadeOut.SetDelay (fadeInScreen.FadeOutDelay);
						fadeOut.SetEase (Ease.OutQuart);
					});
				}
			}).AddTo (this.Disposer).AddTo (fadeInScreen.Disposer);
		}).AddTo (this.Disposer);
	}
}
