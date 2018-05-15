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

			exchangePosition.IsOn.DistinctUntilChanged ().Where (b => b == true).Subscribe (_ => {
				if (exchangePosition.Origins == null || exchangePosition.Targets == null || exchangePosition.Origins.Length <= 0 || exchangePosition.Origins.Length != exchangePosition.Targets.Length) {
					exchangePosition.IsOn.Value = false;
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
						exchangePosition.IsOn.Value = false;
					});
				}
			}).AddTo (this.Disposer).AddTo (exchangePosition.Disposer);

			exchangePosition.OnPointerClickAsObservable ().Where (_ => !exchangePosition.IsOn.Value).Subscribe (_ => {
				exchangePosition.IsOn.Value = true;
			}).AddTo (this.Disposer).AddTo (exchangePosition.Disposer);
		}).AddTo (this.Disposer);
	}
}
