using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

[RequireComponent (typeof(ScrollRect))]
public class CenterOnChild : MonoBehaviour,IBeginDragHandler,IEndDragHandler
{
	public Transform Center;
	[Range (0, 20)]
	public float CenterSpeed = 5f;
	private ScrollRect scrollView;
	private List<Coroutine> centerCoroutines = new List<Coroutine> ();

	public delegate void OnCenterHandler (Transform centerChild);

	public event OnCenterHandler onCenter;

	void Start ()
	{
		if (Center == null) {
			Center = transform;
		}
		scrollView = GetComponent<ScrollRect> ();

		for (int i = 0; i < scrollView.content.childCount; i++) {
			var child = scrollView.content.GetChild (i);
			EventTriggerListener.Get (child.gameObject).PointerClick += (eventData => {
				if (!eventData.dragging) {
					StopCentering ();
					centerCoroutines.Add (StartCoroutine (CenterAsync (child)));
				}
			});

			EventTriggerListener.Get (child.gameObject).BeginDrag += (eventData => {
				scrollView.OnBeginDrag (eventData);
				StopCentering ();
			});

			EventTriggerListener.Get (child.gameObject).Drag += (eventData => {
				scrollView.OnDrag (eventData);
			});

			EventTriggerListener.Get (child.gameObject).EndDrag += (eventData => {
				scrollView.OnEndDrag (eventData);
				if (Center != null) {
					StartCentering ();
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
		if (Center != null) {
			StartCentering ();
		}
	}

	void StartCentering ()
	{
		var child = FindClosestChild ();
		centerCoroutines.Add (StartCoroutine (CenterAsync (child)));
	}

	IEnumerator CenterAsync (Transform target)
	{
		var offset = target.position - scrollView.content.position;

		scrollView.velocity = Vector2.zero;

		while (Vector3.Magnitude (scrollView.content.position + offset - Center.position) > 0.01f) {
			scrollView.content.position = Vector3.Lerp (scrollView.content.position + offset, Center.position, CenterSpeed * Time.deltaTime) - offset;
			yield return null;	
		}
		scrollView.content.position = Center.position - offset;
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

	Transform FindClosestChild ()
	{
		var direction = scrollView.velocity.normalized;
		var childIndex = (scrollView.horizontal && direction.x > 0) || (scrollView.vertical && direction.y > 0) ? 0 : scrollView.content.childCount - 1;
		var distance = Mathf.Infinity;

		for (int i = 0; i < scrollView.content.childCount; i++) {
			var child = scrollView.content.GetChild (i);
			var vector = Vector2.zero;
			if (scrollView.inertia) {
				var velocity = scrollView.velocity;
				while (velocity.magnitude > 1) {
					velocity *= Mathf.Pow (scrollView.decelerationRate, Time.unscaledDeltaTime);
					vector += velocity * Time.unscaledDeltaTime;
				}
			}
			var pos = child.position + child.TransformVector (new Vector3 (vector.x, vector.y, 0));
			if (scrollView.horizontal) {
				var dir = Vector3.Project (pos - Center.position, Center.right);
				if (direction.x > 0 && dir.x > 0) {
					continue;
				}
				if (direction.x < 0 && dir.x < 0) {
					continue;
				}
			}
			if (scrollView.vertical) {
				var dir = Vector3.Project (pos - Center.position, Center.up);
				if (direction.y > 0 && dir.y > 0) {
					continue;
				}
				if (direction.y < 0 && dir.y < 0) {
					continue;
				}
			}

			var dis = Vector3.Distance (pos, Center.position);
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
