using UnityEngine.EventSystems;
using UnityEngine.UI;
using UniRx.Triggers;
using UnityEngine;
using DG.Tweening;
using UniRx;

public class FadeInScreen : MonoBehaviour
{
	public CanvasGroup Mask;
	[Range (0, 1)]
	public float Alpha = 1;
	[Range (0, 1)]
	public float Duration = 0.2f;
	[Range (0, 1)]
	public float FadeInDelay = 0;
	[Range (0, 1)]
	public float FadeOutDelay = 0;

	void Start ()
	{
		gameObject.OnPointerClickAsObservable ().Subscribe (OnPointerClick);
	}

	void OnPointerClick (PointerEventData eventData)
	{
		if (Mask != null) {
			var originAlpha = Mask.alpha;
			var fadeIn = Mask.DOFade (Alpha, Duration);
			fadeIn.SetDelay (FadeInDelay);
			fadeIn.SetEase (Ease.InQuart);
			fadeIn.OnComplete (() => {
				var fadeOut = Mask.DOFade (originAlpha, Duration);
				fadeOut.SetDelay (FadeOutDelay);
				fadeOut.SetEase (Ease.OutQuart);
			});
		}
	}
}
