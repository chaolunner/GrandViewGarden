//using UnityEngine.EventSystems;
//using UniRx.Triggers;
using UnityEngine;
//using DG.Tweening;
using UniECS;
//using UniRx;

public class PunchButton : ComponentBehaviour
{
	[Range (0, 2)]
	public float Scale = 1.2f;
	[Range (0, 1)]
	public float Duration = 0.2f;

//	void Start ()
//	{
//		gameObject.OnPointerDownAsObservable ().Subscribe (PointerDown);
//	}
//
//	void PointerDown (PointerEventData eventData)
//	{
//		var originScale = transform.localScale;
//		var tweener = transform.DOScale (Scale * Vector2.one, Duration);
//		tweener.OnComplete (() => {
//			transform.DOScale (originScale, Duration);
//		});
//	}
}
