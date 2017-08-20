using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor (typeof(FreeDriving))]
public class FreeDrivingDrawer : Editor
{
	public FreeDriving freeDriving;

	void OnEnable ()
	{
		freeDriving = target as FreeDriving;
	}

	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector ();

		EditorGUI.BeginChangeCheck ();
		var index = EditorGUILayout.Popup ("Pose Index", freeDriving.PoseIndex.Value, freeDriving.options.Select (x => x.ToString ()).ToArray ());
		if (EditorGUI.EndChangeCheck ()) {
			Undo.RecordObject (freeDriving, "Change PoseIndex");
			freeDriving.PoseIndex.Value = index;
			EditorUtility.SetDirty (freeDriving);
		}
	}
}
