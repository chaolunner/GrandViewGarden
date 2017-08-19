using UniRx.Triggers;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UniECS;
using System;
using UniRx;

public class FadeInCenterSystem : SystemBehaviour
{
	public override void Awake ()
	{
		base.Awake ();

		var FadeInCenterEntities = GroupFactory.Create (new Type[] {
			typeof(FadeInCenter)
		});
		FadeInCenterEntities.OnAdd ().Subscribe (entity => {	
			
			var fadeInCenter = entity.GetComponent<FadeInCenter> ();

			if (fadeInCenter.Center == null) {
				fadeInCenter.Center = transform as RectTransform;
			}
			fadeInCenter.scrollView = GetComponent<ScrollRect> ();
			fadeInCenter.gridLayoutGroup = fadeInCenter.scrollView.content.GetComponent<GridLayoutGroup> ();

			fadeInCenter.centerCollider = gameObject.AddComponent<BoxCollider2D> ();
			fadeInCenter.centerCollider.size = fadeInCenter.gridLayoutGroup.cellSize;
			fadeInCenter.centerCollider.isTrigger = true;

			for (int i = 0; i < fadeInCenter.scrollView.content.childCount; i++) {
				var child = fadeInCenter.scrollView.content.GetChild (i) as RectTransform;
				var col = child.gameObject.AddComponent<BoxCollider2D> ();
				var rig = child.gameObject.AddComponent<Rigidbody2D> ();
				var canvasGroup = child.GetComponentInChildren<CanvasGroup> ();
				if (canvasGroup != null) {
					fadeInCenter.components.Add (child, canvasGroup);
					canvasGroup.alpha = 0;
				}
				rig.sleepMode = RigidbodySleepMode2D.NeverSleep;
				col.size = fadeInCenter.gridLayoutGroup.cellSize;
				col.isTrigger = true;
				rig.gravityScale = 0;
			}

			fadeInCenter.OnTriggerStay2DAsObservable ().Subscribe (col => {
				if (col.gameObject.layer == LayerMask.NameToLayer ("UI")) {
					var k = 1f;
					var distance = Vector3.Distance (col.transform.position, fadeInCenter.Center.position);
					if (fadeInCenter.scrollView.horizontal) {
						k -= Mathf.Clamp01 (distance / fadeInCenter.centerCollider.bounds.size.x);
					} else {
						k -= Mathf.Clamp01 (distance / fadeInCenter.centerCollider.bounds.size.y);
					}
					col.transform.localScale = (1 + k * (fadeInCenter.Scale - 1)) * Vector3.one;

					if (!fadeInCenter.components.ContainsKey (col.transform)) {
						var canvasGroup = col.GetComponentInChildren<CanvasGroup> ();
						if (canvasGroup != null) {
							fadeInCenter.components.Add (col.transform, canvasGroup);
						}
					}
					if (fadeInCenter.components.ContainsKey (col.transform)) {
						var canvasGroup = fadeInCenter.components [col.transform];
						if (canvasGroup != null) {
							canvasGroup.alpha = k;
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
