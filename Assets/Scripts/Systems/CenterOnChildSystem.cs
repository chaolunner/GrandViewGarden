using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using UniRx.Triggers;
using UnityEngine;
using UniECS;
using System;
using UniRx;

public class CenterOnChildSystem : SystemBehaviour
{
	public override void Awake ()
	{
		base.Awake ();

//		var CenterOnChildEntities = GroupFactory.Create (new Type[] {
//			typeof(CenterOnChild)
//		});
//
//		CenterOnChildEntities.OnAdd ().Subscribe (entity => {
//			var centerOnCenter = entity.GetComponent<CenterOnChild> ();
//
//			if (centerOnCenter.Center == null) {
//				centerOnCenter.Center = transform;
//			}
//			centerOnCenter.scrollView = GetComponent<ScrollRect> ();
//			centerOnCenter.gridLayoutGroup = centerOnCenter.scrollView.content.GetComponent<GridLayoutGroup> ();
//
//			for (int i = 0; i < centerOnCenter.scrollView.content.childCount; i++) {
//				var child = centerOnCenter.scrollView.content.GetChild (i);
//				child.OnPointerClickAsObservable ().Subscribe (eventData => {
//					if (!eventData.dragging) {
//						StopCentering ();
//						centerOnCenter.centerCoroutines.Add (StartCoroutine (CenterAsync (child)));
//					}
//				});
//
//				child.OnBeginDragAsObservable ().Subscribe (eventData => {
//					centerOnCenter.scrollView.OnBeginDrag (eventData);
//					StopCentering ();
//				});
//
//				child.OnDragAsObservable ().Subscribe (eventData => {
//					centerOnCenter.scrollView.OnDrag (eventData);
//				});
//
//				child.OnEndDragAsObservable ().Subscribe (eventData => {
//					centerOnCenter.scrollView.OnEndDrag (eventData);
//					if (Center != null) {
//						StartCentering ();
//					}				
//				});					
//			}
//
//			centerOnCenter.OnBeginDragAsObservable ().Subscribe (eventData => {
//				for (int i = 0; i < centerCoroutines.Count; i++) {
//					if (centerCoroutines [i] != null) {
//						StopCoroutine (centerOnCenter.centerCoroutines [i]);
//					}
//				}
//				centerCoroutines.Clear ();
//			});
//			centerOnCenter.OnEndDragAsObservable().Subscribe(eventData =>{
//				
//			});
//		
//		});
	}
}
