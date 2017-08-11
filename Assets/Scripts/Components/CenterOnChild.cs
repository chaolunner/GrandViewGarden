using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

[RequireComponent (typeof(ScrollRect))]
public class CenterOnChild : MonoBehaviour,IBeginDragHandler,IEndDragHandler
{
	public Transform center;
	[Range (0, 20)]
	public float centerSpeed = 5f;
	private ScrollRect scrollView;
	private GridLayoutGroup gridLayoutGroup;
	private List<Coroutine> centerCoroutines = new List<Coroutine> ();

	public delegate void OnCenterHandler (Transform centerChild);

	public event OnCenterHandler onCenter;

	void Start ()
	{
		if (center == null) {
			center = transform;
		}
		scrollView = GetComponent<ScrollRect> ();
		gridLayoutGroup = scrollView.content.GetComponent<GridLayoutGroup> ();

		for (int i = 0; i < scrollView.content.childCount; i++) {
			var child = scrollView.content.GetChild (i);
			EventTriggerListener.Get (child.gameObject).PointerClick = (eventData => {
				if (!eventData.dragging) {
					StopCentering ();
					centerCoroutines.Add (StartCoroutine (CenterAsync (child)));
				}
			});

			EventTriggerListener.Get (child.gameObject).BeginDrag = (eventData => {
				scrollView.OnBeginDrag (eventData);
				StopCentering ();
			});

			EventTriggerListener.Get (child.gameObject).Drag = (eventData => {
				scrollView.OnDrag (eventData);
			});

			EventTriggerListener.Get (child.gameObject).EndDrag = (eventData => {
				scrollView.OnEndDrag (eventData);
				if (center != null) {
					centerCoroutines.Add (StartCoroutine (CenterAsync ()));
				}
			});
		}
	}

	public void OnBeginDrag (PointerEventData eventData)
	{
		StopCentering ();
	}

	public void OnEndDrag (PointerEventData eventData)
	{
		if (center != null) {
			centerCoroutines.Add (StartCoroutine (CenterAsync ()));
		}
	}

	IEnumerator CenterAsync ()
	{
		while (scrollView.velocity.x >= 2 * gridLayoutGroup.cellSize.x + gridLayoutGroup.spacing.x ||
		       scrollView.velocity.y >= 2 * gridLayoutGroup.cellSize.y + gridLayoutGroup.spacing.y) {
			yield return null;
		}

		var child = FindClosestChild (scrollView.velocity.normalized);
		centerCoroutines.Add (StartCoroutine (CenterAsync (child)));
	}

	IEnumerator CenterAsync (Transform target)
	{
		var offset = target.position - scrollView.content.position;

		scrollView.velocity = Vector2.zero;

		while (Vector3.Magnitude (scrollView.content.position + offset - center.position) > 0.01f) {
			scrollView.content.position = Vector3.Lerp (scrollView.content.position + offset, center.position, centerSpeed * Time.deltaTime) - offset;
			yield return null;	
		}
		scrollView.content.position = center.position - offset;
	}

	void StopCentering ()
	{
		for (int i = 0; i < centerCoroutines.Count; i++) {
			if (centerCoroutines [i] != null) {
				StopCoroutine (centerCoroutines [i]);
			}
		}
		centerCoroutines.Clear ();
	}

	Transform FindClosestChild (Vector2 direction)
	{
		var childIndex = (scrollView.horizontal && direction.x > 0) || (scrollView.vertical && direction.y > 0) ? 0 : scrollView.content.childCount - 1;
		var distance = Mathf.Infinity;

		for (int i = 0; i < scrollView.content.childCount; i++) {
			var pos = scrollView.content.GetChild (i).position;
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

		if (onCenter != null) {
			onCenter (centerChild);
		}

		return centerChild;
	}
}
