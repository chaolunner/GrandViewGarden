using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor (typeof(Timing))]
public class TimingDrawer : Editor
{
	public Timing timing;

	void OnEnable ()
	{
		timing = target as Timing;
	}

	public override void OnInspectorGUI ()
	{
		EditorGUILayout.LabelField ("Time", timing.Hour+":"+timing.Minute+":"+timing.Second+":"+timing.MilliSecond);
	}

}
