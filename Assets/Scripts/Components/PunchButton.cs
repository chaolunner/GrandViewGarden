using UnityEngine.EventSystems;
using UnityEngine;
using DG.Tweening;

public class PunchButton : MonoBehaviour
{
	[Range (0, 2)]
	public float Scale = 1.2f;
	[Range (0, 1)]
	public float Duration = 0.2f;

	void OnEnable ()
	{
		EventTriggerListener.Get (gameObject).PointerDown += PointerDown;
	}

	void OnDisable ()
	{
		EventTriggerListener.Get (gameObject).PointerDown -= PointerDown;
	}

	void Start ()
	{
		
	}

	void PointerDown (PointerEventData eventData)
	{
		var originScale = transform.localScale;
		var tweener = transform.DOScale (Scale * Vector2.one, Duration);
		tweener.OnComplete (() => {
			transform.DOScale (originScale, Duration);
		});
	}
}
