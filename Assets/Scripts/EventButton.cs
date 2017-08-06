using UnityEngine.EventSystems;
using UnityEngine;
using DG.Tweening;

public class EventButton : MonoBehaviour
{
	private GameObject button;
	[Range (0, 1)]
	public float durationButton = 0.5f;

	void Start ()
	{
		EventTriggerListener.Get (gameObject).PointerDown = PointerDown;

	}

	void PointerDown (PointerEventData eventData)
	{
		eventData.selectedObject.transform.DOScale (new Vector2 (2, 2), durationButton);
		eventData.selectedObject.transform.DOScale (new Vector2 (1, 1), durationButton).SetDelay (0.5f);

	}
		
}
