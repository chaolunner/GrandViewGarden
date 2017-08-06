using UnityEngine.EventSystems;
using UnityEngine;
using System;

public class EventTriggerListener : EventTrigger
{
	public Action<PointerEventData> PointerEnter;
	public Action<PointerEventData> PointerDown;
	public Action<PointerEventData> PointerClick;
	public Action<PointerEventData> PointerUp;
	public Action<PointerEventData> PointerExit;
	public Action<PointerEventData> BeginDrag;
	public Action<PointerEventData> Drag;
	public Action<PointerEventData> EndDrag;
	public Action<BaseEventData> Select;
	public Action<BaseEventData> UpdateSelected;

	static public EventTriggerListener Get (GameObject go)
	{
		EventTriggerListener listener = go.GetComponent<EventTriggerListener> ();
		if (listener == null) {
			listener = go.AddComponent<EventTriggerListener> ();
		}
		return listener;
	}

	public override void OnPointerClick (PointerEventData eventData)
	{
		if (PointerClick != null) {
			PointerClick (eventData);
		}
	}

	public override void OnPointerDown (PointerEventData eventData)
	{
		if (PointerDown != null) {
			PointerDown (eventData);
		}
	}

	public override void OnPointerEnter (PointerEventData eventData)
	{
		if (PointerEnter != null) {
			PointerEnter (eventData);
		}
	}

	public override void OnPointerExit (PointerEventData eventData)
	{
		if (PointerExit != null) {
			PointerExit (eventData);
		}
	}

	public override void OnPointerUp (PointerEventData eventData)
	{
		if (PointerUp != null) {
			PointerUp (eventData);
		}
	}

	public override void OnBeginDrag (PointerEventData eventData)
	{
		if (BeginDrag != null) {
			BeginDrag (eventData);
		}
	}

	public override void OnDrag (PointerEventData eventData)
	{
		if (Drag != null) {
			Drag (eventData);
		}
	}

	public override void OnEndDrag (PointerEventData eventData)
	{
		if (EndDrag != null) {
			EndDrag (eventData);
		}
	}

	public override void OnSelect (BaseEventData eventData)
	{
		if (Select != null) {
			Select (eventData);
		}
	}

	public override void OnUpdateSelected (BaseEventData eventData)
	{
		if (UpdateSelected != null) {
			UpdateSelected (eventData);
		}
	}
}
