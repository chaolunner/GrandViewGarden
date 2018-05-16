using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor (typeof(InteractiveComponent))]
public class InteractiveComponentDrawer : Editor
{
	public bool Foldout;
	public InteractiveComponent InteractiveComponent;
	public List<Transform> TouchAreas = new List<Transform> ();

	void OnEnable ()
	{
		InteractiveComponent = target as InteractiveComponent;
	}

	public override void OnInspectorGUI ()
	{
		EditorGUI.BeginChangeCheck ();
		TouchAreas.Clear ();
		TouchAreas.AddRange (InteractiveComponent.TouchAreas);
		Foldout = EditorGUILayout.Foldout (Foldout, "Touch Areas");
		if (Foldout) {
			EditorGUI.indentLevel++;
			for (int i = 0; i < TouchAreas.Count; i++) {
				var touchArea = TouchAreas [i];
				var rect = EditorGUILayout.GetControlRect ();
				rect = EditorGUI.PrefixLabel (rect, new GUIContent (touchArea == null ? "Empty" : touchArea.name));
				rect.xMax -= 18;
				touchArea = (Transform)EditorGUI.ObjectField (rect, touchArea, typeof(Transform), true);
				rect.xMin = rect.xMax;
				rect.xMax += 18;
				if (GUI.Button (rect, "-")) {
					TouchAreas.RemoveAt (i);
					break;
				}
				TouchAreas [i] = touchArea;
			}

			if (GUILayout.Button ("Add Touch Area")) {
				var lastTouchArea = TouchAreas.LastOrDefault ();
				TouchAreas.Add (lastTouchArea);
			}
			EditorGUI.indentLevel--;
		}
		if (EditorGUI.EndChangeCheck ()) {
			Undo.RecordObject (InteractiveComponent, "Modify Touch Areas");
			InteractiveComponent.TouchAreas = TouchAreas.ToArray ();
			EditorUtility.SetDirty (InteractiveComponent);
		}
	}
}
