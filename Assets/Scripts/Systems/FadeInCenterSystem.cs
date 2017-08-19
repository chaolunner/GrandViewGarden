using System.Collections.Generic;
using UnityEngine.UI;
using UniRx.Triggers;
using UnityEngine;
using System;
using UniECS;
using UniRx;

public class FadeInCenterSystem : SystemBehaviour
{
	public override void Awake ()
	{
		base.Awake ();

		var FadeInCenterEntities = GroupFactory.Create (new Type[] {
			typeof(RectTransform),
			typeof(FadeInCenter),
			typeof(ScrollRect),
		});

		FadeInCenterEntities.OnAdd ().Subscribe (entity => {
			var fadeInCenter = entity.GetComponent<FadeInCenter> ();
			var scrollView = entity.GetComponent<ScrollRect> ();
			var items = new Dictionary<Transform, CanvasGroup> ();

			var center = scrollView.viewport;
			var gridLayoutGroup = scrollView.content.GetComponent<GridLayoutGroup> ();
			var centerCollider = scrollView.gameObject.AddComponent<BoxCollider2D> ();

			centerCollider.size = gridLayoutGroup.cellSize;
			centerCollider.isTrigger = true;

			for (int i = 0; i < scrollView.content.childCount; i++) {
				var child = scrollView.content.GetChild (i) as RectTransform;
				var col = child.gameObject.AddComponent<BoxCollider2D> ();
				var rig = child.gameObject.AddComponent<Rigidbody2D> ();
				var canvasGroup = child.GetComponentInChildren<CanvasGroup> ();

				if (canvasGroup != null) {
					items.Add (child, canvasGroup);
					canvasGroup.alpha = 0;
				}

				rig.sleepMode = RigidbodySleepMode2D.NeverSleep;
				col.size = gridLayoutGroup.cellSize;
				col.isTrigger = true;
				rig.gravityScale = 0;
			}

			fadeInCenter.OnTriggerStay2DAsObservable ().Subscribe (col => {
				if (col.gameObject.layer == LayerMask.NameToLayer ("UI")) {
					var k = 1f;
					var distance = Vector3.Distance (col.transform.position, center.position);

					if (scrollView.horizontal) {
						k -= Mathf.Clamp01 (distance / centerCollider.bounds.size.x);
					} else {
						k -= Mathf.Clamp01 (distance / centerCollider.bounds.size.y);
					}

					col.transform.localScale = (1 + k * (fadeInCenter.Scale - 1)) * Vector3.one;

					if (!items.ContainsKey (col.transform)) {
						var canvasGroup = col.GetComponentInChildren<CanvasGroup> ();
						if (canvasGroup != null) {
							items.Add (col.transform, canvasGroup);
						}
					}

					if (items.ContainsKey (col.transform)) {
						var canvasGroup = items [col.transform];
						if (canvasGroup != null) {
							canvasGroup.alpha = Mathf.Clamp (k, fadeInCenter.AlphaRange.min, fadeInCenter.AlphaRange.max);
						}
					}
				}
			});

			fadeInCenter.OnTriggerExit2DAsObservable ().Subscribe (col => {
				if (col.gameObject.layer == LayerMask.NameToLayer ("UI")) {
					col.transform.localScale = Vector3.one;
				}
			});
		});
	}
}
