using UnityEngine;
using DG.Tweening;
using UniECS;

[CreateAssetMenu (menuName = "Preferences/Split")]
public class Split : FadeInSequence
{
	public override Sequence GetSequence (IEntity entity, float endValue)
	{
		if (entity.HasComponent<InteractiveComponent> ()) {
			var fadeIn = entity.GetComponent<FadeIn> ();
			var interactiveComponent = entity.GetComponent<InteractiveComponent> ();
			var topTransform = interactiveComponent.TouchAreas [0] as RectTransform;
			var bottomTransform = interactiveComponent.TouchAreas [1] as RectTransform;
			var max = Mathf.Max (topTransform.localScale.y, bottomTransform.localScale.y);
			var duration = Mathf.Abs (endValue - max) * fadeIn.Duration;
			var sequence = DOTween.Sequence ();

			sequence.Join (topTransform.DOScaleY (endValue, duration));
			sequence.Join (bottomTransform.DOScaleY (endValue, duration));
			if (endValue > max) {
				sequence.SetEase (Ease.InQuart);
			} else {
				sequence.SetEase (Ease.OutQuart);
			}
		}
		return null;
	}
}
