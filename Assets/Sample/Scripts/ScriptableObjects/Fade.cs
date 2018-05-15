using UnityEngine;
using DG.Tweening;
using UniECS;

[CreateAssetMenu (menuName = "Preferences/Fade")]
public class Fade : FadeInSequence
{
	public override Sequence GetSequence (IEntity entity, float endValue)
	{
		if (entity.HasComponent<CanvasGroup> ()) {
			var fadeIn = entity.GetComponent<FadeIn> ();
			var canvasGroup = entity.GetComponent<CanvasGroup> ();
			var duration = Mathf.Abs (endValue - canvasGroup.alpha) * fadeIn.Duration;
			var sequence = DOTween.Sequence ();

			sequence.Append (canvasGroup.DOFade (endValue, duration));
			if (endValue > canvasGroup.alpha) {
				sequence.SetEase (Ease.InQuart);
			} else {
				sequence.SetEase (Ease.OutQuart);
			}
		}
		return null;
	}
}
