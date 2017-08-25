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
			typeof(FadeIn)
		});

		var ExchangePositionEntities = GroupFactory.Create (new Type[] {
			typeof(ExchangePosition)
		});

		FadeInEntities.OnAdd ().Subscribe (entity => {
			var fadeIn = entity.GetComponent<FadeIn> ();
			Sequence sequence = null;

			fadeIn.NormalizedTime.DistinctUntilChanged ()
				.Subscribe (time => {
				sequence.Kill ();
				sequence = fadeIn.Sequence.GetSequence (entity, time);
			}).AddTo (this.Disposer).AddTo (fadeIn.Disposer);

			if (fadeIn.FadeInType == FadeInType.Panel) {
				ExchangePositionEntities.OnAdd ().Subscribe (exchangePositionEntity => {
					var exchangePosition = exchangePositionEntity.GetComponent<ExchangePosition> ();

					exchangePosition.IsOn.DistinctUntilChanged ().Where (b => b == true && exchangePosition.Duration > 0).TakeWhile (_ => fadeIn != null).Subscribe (unused => {
						fadeIn.NormalizedTime.Value = 1;
						Observable.Timer (TimeSpan.FromSeconds (0.6f)).Subscribe (_ => {
							fadeIn.NormalizedTime.Value = 0;
						}).AddTo (this.Disposer).AddTo (exchangePosition.Disposer).AddTo (fadeIn.Disposer);
					});
				}).AddTo (this.Disposer);
			}
		}).AddTo (this.Disposer);
	}
}
