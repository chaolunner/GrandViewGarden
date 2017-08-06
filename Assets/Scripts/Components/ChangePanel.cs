using UnityEngine.EventSystems;
using UnityEngine;
using DG.Tweening;

public class ChangePanel : MonoBehaviour
{
	public RectTransform originPanel;
	public RectTransform targetpanel;
	[Range (0, 1)]
	public float duration = 0.5f;

	void Start ()
	{
		EventTriggerListener.Get (gameObject).PointerClick = OnPointerClick;
	}

	void OnPointerClick (PointerEventData eventData)
	{
		if (originPanel == null || targetpanel == null) {
			return;
		}
		var originPosition = originPanel.position;
		var tweener = originPanel.DOMove (targetpanel.position, duration);
		tweener.SetEase (Ease.InBack);
		tweener.OnComplete (() => {
			targetpanel.DOMove (originPosition, duration).SetEase (Ease.OutBack);
		});
	}
}
