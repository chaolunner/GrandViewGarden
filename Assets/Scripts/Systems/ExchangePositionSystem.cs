using UniRx.Triggers;
using UnityEngine;
using DG.Tweening;
using UniECS;
using System;
using UniRx;

public class ExchangePositionSystem : SystemBehaviour
{
	public override void Awake ()
	{
		base.Awake ();

		var ExchangePositionEntities = GroupFactory.Create (new Type[] {
			typeof(ExchangePosition)
		});

		ExchangePositionEntities.OnAdd ().Subscribe (entity => {
			var exchangePosition = entity.GetComponent<ExchangePosition> ();

			exchangePosition.DoExchange.Select (_ => true)
				.Merge (exchangePosition.OnPointerClickAsObservable ().Select (_ => true))
				.Subscribe (_ => {
				if (exchangePosition.Origins == null || exchangePosition.Targets == null || exchangePosition.Origins.Length <= 0 || exchangePosition.Origins.Length != exchangePosition.Targets.Length) {
					return;
				}
				for (int i = 0; i < exchangePosition.Origins.Length; i++) {
					var index = i;
					var originPosition = exchangePosition.Origins [index].position;
					var tweener = exchangePosition.Origins [index].DOMove (exchangePosition.Targets [index].position, exchangePosition.Duration);
					tweener.SetDelay (exchangePosition.Delay);
					tweener.SetEase (Ease.InBack);
					tweener.OnComplete (() => {
						exchangePosition.Targets [index].DOMove (originPosition, exchangePosition.Duration).SetEase (Ease.OutBack);
					});
				}
			}).AddTo (this.Disposer).AddTo (exchangePosition.Disposer);
		}).AddTo (this.Disposer);
	}
}
