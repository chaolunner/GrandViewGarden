using System.Collections.Generic;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor (typeof(FPSUtility))]
public class FPSUtilityDrawer : Editor
{
	public bool Foldout;
	public FPSUtility FpsUtility;
	public List<FPSColor> FpsColorList = new List<FPSColor> ();

	void OnEnable ()
	{
		FpsUtility = target as FPSUtility;
	}

	public override void OnInspectorGUI ()
	{
		EditorGUI.BeginChangeCheck ();
		var frameRange = EditorGUILayout.IntField ("Frame Range", FpsUtility.frameRange);
		var highestFPSLabel = (Text)EditorGUILayout.ObjectField ("Highest FPS Label", FpsUtility.highestFPSLabel, typeof(Text), true);
		var averageFPSLabel = (Text)EditorGUILayout.ObjectField ("Average FPS Label", FpsUtility.averageFPSLabel, typeof(Text), true);
		var lowestFPSLabel = (Text)EditorGUILayout.ObjectField ("Lowest FPS Label", FpsUtility.lowestFPSLabel, typeof(Text), true);

		FpsColorList.Clear ();
		FpsColorList.AddRange (FpsUtility.coloring);

		Foldout = EditorGUILayout.Foldout (Foldout, "Edit Color Setting");
		if (Foldout) {
			EditorGUI.indentLevel++;
			for (int i = 0; i < FpsColorList.Count; i++) {
				var fpsColor = FpsColorList [i];
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
					FpsColorList.RemoveAt (i);
					break;
				}
				FpsColorList [i] = fpsColor;
			}
			
			if (GUILayout.Button ("Add Color List")) {
				var last = FpsColorList.LastOrDefault ();
				var newest = new FPSColor ();
				newest.minimumFPS = last.minimumFPS;
				newest.color = last.color;
				FpsColorList.Add (newest);
			}
			EditorGUI.indentLevel--;
		}

		if (EditorGUI.EndChangeCheck ()) {
			Undo.RecordObject (FpsUtility, "Change FPS Setting");
			FpsUtility.frameRange = frameRange;
			FpsUtility.highestFPSLabel = highestFPSLabel;
			FpsUtility.averageFPSLabel = averageFPSLabel;
			FpsUtility.lowestFPSLabel = lowestFPSLabel;
			FpsUtility.coloring = FpsColorList.ToArray ();
			EditorUtility.SetDirty (FpsUtility);
		}
	}
}
