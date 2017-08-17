using UnityEngine.EventSystems;
using UniRx.Triggers;
using UnityEngine;
using DG.Tweening;
using UniRx;

public class ExchangePosition : MonoBehaviour
{
	public Transform Origin;
	public Transform Target;
	[Range (0, 1)]
	public float Duration = 0.5f;

	[Range (0, 2)]
	public float Delay = 0;

	void Start ()
	{
		gameObject.OnPointerClickAsObservable ().Subscribe (OnPointerClick);
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
			Target.gameObject.SetActive (true);
			Origin.gameObject.SetActive (false);
			Target.DOMove (originPosition, Duration).SetEase (Ease.OutBack);
		});
	}
}
