using UniRx.Triggers;
using DG.Tweening;
using UnityEngine;
using System.Linq;
using UniECS;
using System;
using UniRx;

public class FadeInSystem : SystemBehaviour
{
	public override void Awake ()
	{
		base.Awake ();

		var FadeInEntities = GroupFactory.Create (new Type[] {
			typeof(CanvasGroup),
			typeof(FadeInTag),
			typeof(FadeIn),
		});

		var ExchangePositionEntities = GroupFactory.Create (new Type[] {
			typeof(ExchangePosition)
		});

		FadeInEntities.OnAdd ().Subscribe (entity => {
			var canvasGroup = entity.GetComponent<CanvasGroup> ();
			var fadeInTag = entity.GetComponent<FadeInTag> ();
			var fadeIn = entity.GetComponent<FadeIn> ();
			Tweener tweener = null;

			fadeIn.Alpha.DistinctUntilChanged ()
				.Where (alpha => alpha != canvasGroup.alpha)
				.Subscribe (alpha => {
				tweener.Kill ();
				tweener = canvasGroup.DOFade (alpha, fadeIn.Duration);
				if (alpha > canvasGroup.alpha) {
					tweener.SetEase (Ease.InQuart);
				} else {
					tweener.SetEase (Ease.OutQuart);
				}
			}).AddTo (this.Disposer).AddTo (fadeIn.Disposer);

			if (fadeInTag.FadeInType == FadeInType.Panel) {
				ExchangePositionEntities.OnAdd ().Subscribe (exchangePositionEntity => {
					var exchangePosition = exchangePositionEntity.GetComponent<ExchangePosition> ();

					exchangePosition.OnPointerClickAsObservable ().TakeWhile (_ => fadeIn != null).Subscribe (unused => {
						fadeIn.Alpha.Value = 1;
						Observable.Timer (TimeSpan.FromSeconds (0.6f)).Subscribe (_ => {
							fadeIn.Alpha.Value = 0;
						}).AddTo (this.Disposer).AddTo (exchangePosition.Disposer).AddTo (fadeIn.Disposer);
					});
				}).AddTo (this.Disposer);
			} else if (fadeInTag.FadeInType == FadeInType.Scene) {
			}
		}).AddTo (this.Disposer);
	}
}
