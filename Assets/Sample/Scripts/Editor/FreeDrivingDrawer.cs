using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor (typeof(FreeDriving))]
public class FreeDrivingDrawer : Editor
{
	public FreeDriving FreeDriving;

	void OnEnable ()
	{
		FreeDriving = target as FreeDriving;
	}

	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector ();

		EditorGUI.BeginChangeCheck ();
		var index = EditorGUILayout.Popup ("Pose Index", FreeDriving.PoseIndex.Value, FreeDriving.options.Select (x => x.ToString ()).ToArray ());
		if (EditorGUI.EndChangeCheck ()) {
			Undo.RecordObject (FreeDriving, "Change PoseIndex");
			FreeDriving.PoseIndex.Value = index;
			EditorUtility.SetDirty (FreeDriving);
		}
	}
}
