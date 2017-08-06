using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ScrollG : MonoBehaviour,IEndDragHandler,IDragHandler
{
	public float centerSpeed = 9f;

	public delegate void OnCenterHandler (GameObject centerChild);

	public event OnCenterHandler onCenter;

	private ScrollRect _scrollView;
	private Transform _container;
	private List<float> _childrenPos = new List<float> ();
	private float _targetPos;
	private bool _centering = false;


	void Awake ()
	{
		_scrollView = GetComponent<ScrollRect> ();
		if (_scrollView == null) {
			return;
		}
		_container = _scrollView.content;
		GridLayoutGroup grid;
		grid = _container.GetComponent<GridLayoutGroup> ();
		if (grid == null) {
			return;
		}
		_scrollView.movementType = ScrollRect.MovementType.Unrestricted;

		float childPosX = _scrollView.GetComponent<RectTransform> ().rect.width * 0.5f - grid.cellSize.x * 0.5f;
		_childrenPos.Add (childPosX);

		for (int i = 0; i < _container.childCount - 1; i++) {
			childPosX -= grid.cellSize.x + grid.spacing.x;
			_childrenPos.Add (childPosX);
		}
	}


	void Update ()
	{
		if (_centering) {
			Vector3 v = _container.localPosition;
			v.x = Mathf.Lerp (_container.localPosition.x, _targetPos, centerSpeed * Time.deltaTime);
			_container.localPosition = v;
			if (Mathf.Abs (_container.localPosition.x - _targetPos) < 0.01f) {
				_centering = false;
			}
		}
		
	}

	public void OnEndDrag (PointerEventData eventData)
	{
		_centering = true;
		_targetPos = FindClosestPos (_container.localPosition.x);
	}

	public void OnDrag (PointerEventData eventData)
	{
		_centering = false;
	}

	private float FindClosestPos (float currentPos)
	{
		int childIndex = 0;
		float closest = 0;
		float distance = Mathf.Infinity;

		for (int i = 0; i < _childrenPos.Count; i++) {
			float p = _childrenPos [i];
			float d = Mathf.Abs (p - currentPos);
			if (d < distance) {
				distance = d;
				closest = p;
				childIndex = i;
			}
		}
		GameObject centerChild = _container.GetChild (childIndex).gameObject;
		centerChild.transform.DOScale (2, 1);
		GameObject child1 = _container.GetChild (childIndex-1).gameObject;
		child1.transform.DOScale (1, 1);
		GameObject child2 = _container.GetChild (childIndex+1).gameObject;
		child2.transform.DOScale (1, 1);

		if (onCenter != null) {
			onCenter (centerChild);

		}
		return closest;


	}
}
