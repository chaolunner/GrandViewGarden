using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

[RequireComponent (typeof(ScrollRect))]
[RequireComponent (typeof(RectTransform))]
public class CenterAlignment : MonoBehaviour
{
	[Range (0, 3)]
	public float scale = 1.5f;
	[Range (0, 20)]
	public float centerSpeed = 9f;
	public RectTransform center;
	private ScrollRect scrollView;
	private Coroutine centerCoroutine;
	private BoxCollider2D centerCollider;
	private GridLayoutGroup gridLayoutGroup;
	private Dictionary<Transform, Image> components = new Dictionary<Transform, Image> ();

	public delegate void OnCenterHandler (Transform centerChild);

	public event OnCenterHandler onCenter;

	void Start ()
	{
		if (center == null) {
			center = transform as RectTransform;
		}
		scrollView = GetComponent<ScrollRect> ();
		gridLayoutGroup = scrollView.content.GetComponent<GridLayoutGroup> ();

		centerCollider = gameObject.AddComponent<BoxCollider2D> ();
		centerCollider.size = gridLayoutGroup.cellSize;
		centerCollider.isTrigger = true;

		for (int i = 0; i < scrollView.content.childCount; i++) {
			var child = scrollView.content.GetChild (i) as RectTransform;
			var col = child.gameObject.AddComponent<BoxCollider2D> ();
			var rig = child.gameObject.AddComponent<Rigidbody2D> ();
			var img = child.GetComponent<Image> ();
			if (img != null) {
				components.Add (child, img);
				img.color = new Color (img.color.r, img.color.g, img.color.b, 0);
			}
			rig.sleepMode = RigidbodySleepMode2D.NeverSleep;
			col.size = gridLayoutGroup.cellSize;
			col.isTrigger = true;
			rig.gravityScale = 0;
		}
	}

	void OnTriggerStay2D (Collider2D col)
	{
		if (col.gameObject.layer == LayerMask.NameToLayer ("UI")) {
			var k = 1f;
			var distance = Vector3.Distance (col.transform.position, center.position);
			if (scrollView.horizontal) {
				k -= Mathf.Clamp01 (distance / centerCollider.bounds.size.x);
			} else {
				k -= Mathf.Clamp01 (distance / centerCollider.bounds.size.y);
			}
			col.transform.localScale = (1 + k * (scale - 1)) * Vector3.one;

			if (!components.ContainsKey (col.transform)) {
				var image = col.GetComponent<Image> ();
				if (image != null) {
					components.Add (col.transform, image);
				}
			}
			if (components.ContainsKey (col.transform)) {
				var image = components [col.transform];
				if (image != null) {
					var c = image.color;
					image.color = new Color (c.r, c.g, c.b, k);
				}
			}
				
//			if (scrollView.velocity.magnitude <= centerSpeed) {
//				var child = FindClosestChild (scrollView.velocity.normalized);
//				var offset = child.position - scrollView.content.position;
//				centerCoroutine = StartCoroutine (CenterAsync (offset));
//				scrollView.velocity = Vector3.zero;
//			} else if (centerCoroutine != null) {
//				StopCoroutine (centerCoroutine);
//				centerCoroutine = null;
//			}
		}
	}

	void OnTriggerExit2D (Collider2D col)
	{
		if (col.gameObject.layer == LayerMask.NameToLayer ("UI")) {
			col.transform.localScale = Vector3.one;
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

		return centerChild;
	}

	IEnumerator CenterAsync (Vector3 offset)
	{
		while (Vector3.Magnitude (scrollView.content.position + offset - center.position) > 0.01f) {
			scrollView.content.position = Vector3.Lerp (scrollView.content.position + offset, center.position, centerSpeed * Time.deltaTime) - offset;
			yield return null;	
		}
	}
}
