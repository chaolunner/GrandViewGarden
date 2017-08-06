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
		Tweener tweener = originPanel.DOMoveY (-Screen.height, duration).SetEase (Ease.InBack);
		tweener.OnComplete (OnPanelExit);
	}

	void OnPanelExit ()
	{
		targetpanel.DOMoveY (Screen.height / 2, duration).SetEase (Ease.OutBack);
	}
}
