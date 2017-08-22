using UnityEditor;
using UnityEngine;

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
		EditorGUILayout.LabelField ("Time", string.Format ("{0:D2}:{1:D2}:{2:D2}:{3:D3}", timing.Hour, timing.Minute, timing.Second, timing.MilliSecond));
	}
}
