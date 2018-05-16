using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor (typeof(ExchangePosition))]
public class ExchangePositionDrawer : Editor
{
	public bool Foldout;
	public ExchangePosition ExchangePosition;
	public List<Transform> OriginList = new List<Transform> ();
	public List<Transform> TargetList = new List<Transform> ();

	void OnEnable ()
	{
		ExchangePosition = target as ExchangePosition;
	}

	public override void OnInspectorGUI ()
	{
		EditorGUI.BeginChangeCheck ();
		OriginList.Clear ();
		TargetList.Clear ();
		OriginList.AddRange (ExchangePosition.Origins);
		TargetList.AddRange (ExchangePosition.Targets);

		if (GUILayout.Button ("Do Exchange")) {
			ExchangePosition.IsOn.Value = true;
		}
		var duration = EditorGUILayout.FloatField ("Duration", ExchangePosition.Duration);
		var delay = EditorGUILayout.FloatField ("Delay", ExchangePosition.Delay);
		Foldout = EditorGUILayout.Foldout (Foldout, "Setting");
		if (Foldout) {
			EditorGUI.indentLevel++;
			for (int i = 0; i < OriginList.Count; i++) {
				var origin = OriginList [i];
				var target = TargetList [i];
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
					OriginList.RemoveAt (i);
					TargetList.RemoveAt (i);
					break;
				}
				OriginList [i] = origin;
				TargetList [i] = target;
			}

			if (GUILayout.Button ("Add Target")) {
				var originLast = OriginList.LastOrDefault ();
				var targetLast = TargetList.LastOrDefault ();
				OriginList.Add (originLast);
				TargetList.Add (targetLast);
			}
			EditorGUI.indentLevel--;
		}
		if (EditorGUI.EndChangeCheck ()) {
			Undo.RecordObject (ExchangePosition, "Modify Exchange Position");
			ExchangePosition.Duration = duration;
			ExchangePosition.Delay = delay;
			ExchangePosition.Origins = OriginList.ToArray ();
			ExchangePosition.Targets = TargetList.ToArray ();
			EditorUtility.SetDirty (ExchangePosition);
		}
	}
}
