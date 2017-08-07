using UnityEngine.UI;
using UnityEngine;

[RequireComponent (typeof(ScrollRect))]
public class CenterAlignment : MonoBehaviour
{
	public Transform center;
	private ScrollRect scrollView;
	private BoxCollider2D centerCollider;
	private GridLayoutGroup gridLayoutGroup;

	void Start ()
	{
		if (center == null) {
			center = transform;
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
			rig.sleepMode = RigidbodySleepMode2D.NeverSleep;
			col.size = gridLayoutGroup.cellSize;
			col.isTrigger = true;
			rig.gravityScale = 0;
		}
	}

	void OnTriggerStay2D (Collider2D col)
	{
		if (col.gameObject.layer == LayerMask.NameToLayer ("UI")) {
			var distance = Vector3.Distance (col.transform.position, centerCollider.transform.position);
			var k = 1 - (distance / centerCollider.size.x);
			col.transform.localScale = (1 + k * 0.5f) * Vector3.one;
		}
	}

	void OnTriggerExit2D (Collider2D col)
	{
		if (col.gameObject.layer == LayerMask.NameToLayer ("UI")) {
			col.transform.localScale = Vector3.one;
		}
	}
}
