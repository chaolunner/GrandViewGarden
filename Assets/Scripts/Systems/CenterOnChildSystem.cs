using System.Collections;
using UnityEngine.UI;
using UniRx.Triggers;
using UnityEngine;
using System;
using UniECS;
using UniRx;

public class ClickCenterEvent
{
	public Transform Target;

	public ClickCenterEvent (Transform target)
	{
		Target = target;
	}
}

public class CenterOnChildSystem : SystemBehaviour
{
	public override void Awake ()
	{
		base.Awake ();

		var CenterOnChildEntities = GroupFactory.Create (new Type[] {
			typeof(CenterOnChild),
			typeof(ScrollRect),
		});

		CenterOnChildEntities.OnAdd ().DelayFrame (1).Subscribe (entity => {
			var centerOnCenter = entity.GetComponent<CenterOnChild> ();
			var scrollView = entity.GetComponent<ScrollRect> ();
			var gridLayoutGroup = scrollView.content.GetComponent<GridLayoutGroup> ();
			var cancelCenterDisposer = new CompositeDisposable ();
			var center = scrollView.viewport;

			centerOnCenter.Target.Value = scrollView.content.GetChild (0);

			centerOnCenter.Target.DistinctUntilChanged ().Subscribe (target => {
				cancelCenterDisposer.Clear ();
				if (target != null) {
					scrollView.velocity = Vector2.zero;
					var offset = target.position - scrollView.content.position;

					Observable.EveryUpdate ().TakeWhile (_ => Vector3.Distance (target.position, center.position) > 0.01f)
					.DoOnCompleted (() => {
						scrollView.content.position = center.position - offset;
					}).Subscribe (_ => {
						scrollView.content.position = Vector3.Lerp (scrollView.content.position + offset, center.position, centerOnCenter.CenterSpeed * Time.deltaTime) - offset;
					}).AddTo (this.Disposer).AddTo (centerOnCenter.Disposer).AddTo (cancelCenterDisposer);
				}
			}).AddTo (this.Disposer).AddTo (centerOnCenter.Disposer);

			for (int i = 0; i < scrollView.content.childCount; i++) {
				var child = scrollView.content.GetChild (i);

				child.OnPointerClickAsObservable ().TakeWhile (_ => child != null).Subscribe (eventData => {
					if (!eventData.dragging) {
						if (centerOnCenter.Target.Value == child) {
							EventSystem.Publish (new ClickCenterEvent (child));
						}
						centerOnCenter.Target.Value = child;
					}
				}).AddTo (this.Disposer).AddTo (centerOnCenter.Disposer);

				child.OnBeginDragAsObservable ().TakeWhile (_ => child != null).Subscribe (eventData => {
					scrollView.OnBeginDrag (eventData);
					centerOnCenter.Target.Value = null;
				}).AddTo (this.Disposer).AddTo (centerOnCenter.Disposer);

				child.OnDragAsObservable ().TakeWhile (_ => child != null).Subscribe (eventData => {
					scrollView.OnDrag (eventData);
				}).AddTo (this.Disposer).AddTo (centerOnCenter.Disposer);

				child.OnEndDragAsObservable ().TakeWhile (_ => child != null).Subscribe (eventData => {
					scrollView.OnEndDrag (eventData);
					centerOnCenter.Target.Value = FindClosestChild (scrollView, gridLayoutGroup);
				}).AddTo (this.Disposer).AddTo (centerOnCenter.Disposer);					
			}
		}).AddTo (this.Disposer);
	}

	Transform FindClosestChild (ScrollRect scrollView, GridLayoutGroup gridLayoutGroup)
	{
		var direction = scrollView.velocity.normalized;
		var childIndex = (scrollView.horizontal && direction.x > 0) || (scrollView.vertical && direction.y > 0) ? 0 : scrollView.content.childCount - 1;
		var center = scrollView.viewport;
		var distance = Mathf.Infinity;

		for (int i = 0; i < scrollView.content.childCount; i++) {
			var child = scrollView.content.GetChild (i);
			var vector = Vector2.zero;
			if (scrollView.inertia) {
				var velocity = scrollView.velocity;
				var limit = 2 * gridLayoutGroup.cellSize + gridLayoutGroup.spacing;
				var x = Mathf.Clamp (velocity.x, -limit.x, limit.x);
				var y = Mathf.Clamp (velocity.y, -limit.y, limit.y);
				velocity = new Vector2 (x, y);
				while (velocity.magnitude > 1) {
					velocity *= Mathf.Pow (scrollView.decelerationRate, Time.unscaledDeltaTime);
					vector += velocity * Time.unscaledDeltaTime;
				}
			}
			var pos = child.position + scrollView.content.TransformVector (new Vector3 (vector.x, vector.y, 0));
			if (scrollView.horizontal) {
				var dir = Vector3.Project (pos - center.position, center.right);
				if (direction.x > 0 && dir.x > 0) {
					continue;
				}
				if (direction.x < 0 && dir.x < 0) {
					continue;
				}
			}
			if (scrollView.vertical) {
				var dir = Vector3.Project (pos - center.position, center.up);
				if (direction.y > 0 && dir.y > 0) {
					continue;
				}
				if (direction.y < 0 && dir.y < 0) {
					continue;
				}
			}

			var dis = Vector3.Distance (pos, center.position);
			if (dis < distance) {
				distance = dis;
				childIndex = i;
			}
		}

		var centerChild = scrollView.content.GetChild (childIndex);

		return centerChild;
	}
}
