using System.Collections.Generic;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor (typeof(FPSUtility))]
public class FPSUtilityDrawer : Editor
{
	public bool foldout;
	public FPSUtility fpsUtility;
	public List<FPSColor> fpsColorList = new List<FPSColor> ();

	void OnEnable ()
	{
		fpsUtility = target as FPSUtility;
	}

	public override void OnInspectorGUI ()
	{
		EditorGUI.BeginChangeCheck ();
		var frameRange = EditorGUILayout.IntField ("Frame Range", fpsUtility.frameRange);
		var highestFPSLabel = (Text)EditorGUILayout.ObjectField ("Highest FPS Label", fpsUtility.highestFPSLabel, typeof(Text), true);
		var averageFPSLabel = (Text)EditorGUILayout.ObjectField ("Average FPS Label", fpsUtility.averageFPSLabel, typeof(Text), true);
		var lowestFPSLabel = (Text)EditorGUILayout.ObjectField ("Lowest FPS Label", fpsUtility.lowestFPSLabel, typeof(Text), true);

		fpsColorList.Clear ();
		fpsColorList.AddRange (fpsUtility.coloring);

		foldout = EditorGUILayout.Foldout (foldout, "Edit Color Setting");
		if (foldout) {
			EditorGUI.indentLevel++;
			for (int i = 0; i < fpsColorList.Count; i++) {
				var fpsColor = fpsColorList [i];
				var rect = EditorGUILayout.GetControlRect ();
				rect = EditorGUI.PrefixLabel (rect, new GUIContent ("If FPS >= "));
				rect.xMax -= 18;
				var intRect = new Rect (rect);
				var colRect = new Rect (rect);
				var btnRect = new Rect (rect);
				intRect.xMax = intRect.xMin + 0.5f * rect.size.x;
				colRect.xMin = intRect.xMax;
				btnRect.xMin = colRect.xMax;
				btnRect.xMax = btnRect.xMin + 18;
				fpsColor.minimumFPS = EditorGUI.IntField (intRect, fpsColor.minimumFPS);
				fpsColor.color = EditorGUI.ColorField (colRect, fpsColor.color);
				if (GUI.Button (btnRect, "-")) {
					fpsColorList.RemoveAt (i);
					break;
				}
				fpsColorList [i] = fpsColor;
			}
			
			if (GUILayout.Button ("Add Color List")) {
				var last = fpsColorList.LastOrDefault ();
				var newest = new FPSColor ();
				newest.minimumFPS = last.minimumFPS;
				newest.color = last.color;
				fpsColorList.Add (newest);
			}
			EditorGUI.indentLevel--;
		}

		if (EditorGUI.EndChangeCheck ()) {
			Undo.RecordObject (fpsUtility, "Change FPS Setting");
			fpsUtility.frameRange = frameRange;
			fpsUtility.highestFPSLabel = highestFPSLabel;
			fpsUtility.averageFPSLabel = averageFPSLabel;
			fpsUtility.lowestFPSLabel = lowestFPSLabel;
			fpsUtility.coloring = fpsColorList.ToArray ();
			EditorUtility.SetDirty (fpsUtility);
		}
	}
}
