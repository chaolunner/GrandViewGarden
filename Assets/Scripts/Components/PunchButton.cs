using UnityEngine.EventSystems;
using UnityEngine;
using DG.Tweening;

public class PunchButton : MonoBehaviour
{
	[Range (0, 2)]
	public float scale = 2;
	[Range (0, 1)]
	public float duration = 0.5f;

	void Start ()
	{
		EventTriggerListener.Get (gameObject).PointerDown = PointerDown;
	}

	void PointerDown (PointerEventData eventData)
	{
		var originScale = transform.localScale;
		var tweener = transform.DOScale (scale * Vector2.one, duration);
		tweener.OnComplete (() => {
			transform.DOScale (originScale, duration);
		});
	}
}
