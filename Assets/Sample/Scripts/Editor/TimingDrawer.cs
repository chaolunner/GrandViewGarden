using UnityEditor;
using UnityEngine;

[CustomEditor (typeof(Timing))]
public class TimingDrawer : Editor
{
	public Timing Timing;

	void OnEnable ()
	{
		Timing = target as Timing;
	}

	public override void OnInspectorGUI ()
	{
		EditorGUILayout.LabelField ("Time", string.Format ("{0:D2}:{1:D2}:{2:D2}:{3:D3}", Timing.Hour, Timing.Minute, Timing.Second, Timing.MilliSecond));
	}
}
