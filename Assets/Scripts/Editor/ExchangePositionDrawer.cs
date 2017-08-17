using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor (typeof(ExchangePosition))]
public class ExchangePositionDrawer : Editor
{
	public bool foldout;
	public ExchangePosition exchangePosition;
	public List<Transform> originList = new List<Transform> ();
	public List<Transform> targetList = new List<Transform> ();

	void OnEnable ()
	{
		exchangePosition = target as ExchangePosition;
	}

	public override void OnInspectorGUI ()
	{
		EditorGUI.BeginChangeCheck ();
		originList.Clear ();
		targetList.Clear ();
		originList.AddRange (exchangePosition.Origins);
		targetList.AddRange (exchangePosition.Targets);

		var duration = EditorGUILayout.FloatField ("Duration", exchangePosition.Duration);
		var delay = EditorGUILayout.FloatField ("Delay", exchangePosition.Delay);
		foldout = EditorGUILayout.Foldout (foldout, "Setting");
		if (foldout) {
			EditorGUI.indentLevel++;
			for (int i = 0; i < originList.Count; i++) {
				var origin = originList [i];
				var target = targetList [i];
				var rect = EditorGUILayout.GetControlRect ();
				rect.xMax -= 18;
				var changeRect = new Rect (rect);
				var originRect = new Rect (rect);
				var targetRect = new Rect (rect);
				var buttonRect = new Rect (rect);
				var content = EditorGUIUtility.IconContent ("RotateTool On");
				originRect.xMax = originRect.xMin + 0.5f * rect.size.x - 18;
				changeRect.xMin = originRect.xMax;
				changeRect.xMax = originRect.xMax + 36;
				targetRect.xMin = originRect.xMax + 36;
				buttonRect.xMin = targetRect.xMax;
				buttonRect.xMax = buttonRect.xMin + 18;
				origin = (Transform)EditorGUI.ObjectField (originRect, origin, typeof(Transform), true);
				EditorGUI.LabelField (changeRect, content);
				target = (Transform)EditorGUI.ObjectField (targetRect, target, typeof(Transform), true);
				if (GUI.Button (buttonRect, "-")) {
					originList.RemoveAt (i);
					targetList.RemoveAt (i);
					break;
				}
				originList [i] = origin;
				targetList [i] = target;
			}

			if (GUILayout.Button ("Add Target")) {
				var originLast = originList.LastOrDefault ();
				var targetLast = targetList.LastOrDefault ();
				originList.Add (originLast);
				targetList.Add (targetLast);
			}
			EditorGUI.indentLevel--;
		}
		if (EditorGUI.EndChangeCheck ()) {
			Undo.RecordObject (exchangePosition, "Modify Exchange Position");
			exchangePosition.Duration = duration;
			exchangePosition.Delay = delay;
			exchangePosition.Origins = originList.ToArray ();
			exchangePosition.Targets = targetList.ToArray ();
			EditorUtility.SetDirty (exchangePosition);
		}
	}
}
