using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

[RequireComponent (typeof(ScrollRect))]
public class CenterOnChild : MonoBehaviour,IBeginDragHandler,IEndDragHandler
{
	public Transform center;
	[Range (0, 20)]
	public float centerSpeed = 9f;
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
	}

	public void OnEndDrag (PointerEventData eventData)
	{
		if (coroutine != null) {
			StopCoroutine (coroutine);
		}
		if (center != null) {
			var child = FindClosestChild ();
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

	public Transform FindClosestChild ()
	{
		var childIndex = 0;
		var distance = Mathf.Infinity;

		for (int i = 0; i < scrollView.content.childCount; i++) {
			var dis = Vector3.Distance (scrollView.content.GetChild (i).position, center.position);
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
