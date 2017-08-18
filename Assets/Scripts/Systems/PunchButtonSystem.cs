using UniRx.Triggers;
using UnityEngine;
using DG.Tweening;
using UniECS;
using System;
using UniRx;

public class PunchButtonSystem : SystemBehaviour
{
	public override void Awake ()
	{
		base.Awake ();

		var PunchButtonEntities = GroupFactory.Create (new Type[] {
			typeof(PunchButton)
		});

		PunchButtonEntities.OnAdd ().Subscribe (entity => {
			var punchButton = entity.GetComponent<PunchButton> ();

			punchButton.OnPointerDownAsObservable ().Subscribe (_ => {
				var originScale = punchButton.transform.localScale;
				var tweener = punchButton.transform.DOScale (punchButton.Scale * Vector2.one, punchButton.Duration);
				tweener.OnComplete (() => {
					punchButton.transform.DOScale (originScale, punchButton.Duration);
				});
			}).AddTo (this.Disposer).AddTo (punchButton.Disposer);
		}).AddTo (this.Disposer);
	}
}
