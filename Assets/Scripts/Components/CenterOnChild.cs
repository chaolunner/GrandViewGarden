using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

[RequireComponent (typeof(ScrollRect))]
public class CenterOnChild : MonoBehaviour,IBeginDragHandler, IDragHandler,IEndDragHandler
{
	public Transform center;
	[Range (0, 20)]
	public float centerSpeed = 9f;
	private Vector2 delta;
	private ScrollRect scrollView;
	private Coroutine coroutine;

	public delegate void OnCenterHandler (Transform centerChild);

	public event OnCenterHandler onCenter;

	void Start ()
	{
		if (center == null) {
			center = transform;
		}
		scrollView = GetComponent<ScrollRect> ();
		scrollView.movementType = ScrollRect.MovementType.Unrestricted;
	}

	public void OnBeginDrag (PointerEventData eventData)
	{
		if (coroutine != null) {
			StopCoroutine (coroutine);
		}
		if (eventData.delta != Vector2.zero) {
			delta = eventData.delta;
		}
	}

	public void OnDrag (PointerEventData eventData)
	{
		if (eventData.delta != Vector2.zero) {
			delta = eventData.delta;
		}
	}

	public void OnEndDrag (PointerEventData eventData)
	{
		if (coroutine != null) {
			StopCoroutine (coroutine);
		}
		if (eventData.delta != Vector2.zero) {
			delta = eventData.delta;
		}
		if (center != null) {
			var child = FindClosestChild (delta);
			var offset = child.position - scrollView.content.position;
			coroutine = StartCoroutine (CenterAsync (offset));
		}
	}

	public IEnumerator CenterAsync (Vector3 offset)
	{
		while (Vector3.Magnitude (scrollView.content.position + offset - center.position) > 0.01f) {
			scrollView.content.position = Vector3.Lerp (scrollView.content.position + offset, center.position, centerSpeed * Time.deltaTime) - offset;
			yield return null;	
		}
	}

	public Transform FindClosestChild (Vector2 direction)
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

		scrollView.content.GetChild (childIndex - 1).DOScale (1, 1);
		scrollView.content.GetChild (childIndex + 1).DOScale (1, 1);
		return centerChild;
	}

}
