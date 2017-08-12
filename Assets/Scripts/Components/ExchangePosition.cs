using UnityEngine.EventSystems;
using UnityEngine;
using DG.Tweening;

public class ExchangePosition : MonoBehaviour
{
	public Transform Origin;
	public Transform Target;
	[Range (0, 1)]
	public float Duration = 0.5f;

	[Range (0, 2)]
	public float Delay = 0;

	void OnEnable ()
	{
		EventTriggerListener.Get (gameObject).PointerClick += OnPointerClick;
	}

	void OnDisable ()
	{
		EventTriggerListener.Get (gameObject).PointerClick -= OnPointerClick;
	}

	void Start ()
	{
		
	}

	void OnPointerClick (PointerEventData eventData)
	{
		if (Origin == null || Target == null) {
			return;
		}
		var originPosition = Origin.position;
		var tweener = Origin.DOMove (Target.position, Duration);
		tweener.SetDelay (Delay);
		tweener.SetEase (Ease.InBack);
		tweener.OnComplete (() => {
			Target.DOMove (originPosition, Duration).SetEase (Ease.OutBack);
		});
	}
}
