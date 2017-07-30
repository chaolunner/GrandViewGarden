using UnityEditor;
using UnityEngine;

[CustomEditor (typeof(Trapezoid))]
public class TrapezoidDrawer : Editor
{
	public Trapezoid trapezoid;

	void OnEnable ()
	{
		trapezoid = target as Trapezoid;
		if (trapezoid.Mesh == null || trapezoid.Mesh.vertexCount != 8) {
			trapezoid.Mesh = new Mesh ();
			trapezoid.Mesh.vertices = new Vector3[] {
				new Vector3 (-0.5f, -0.5f, -0.5f),
				new Vector3 (0.5f, 0.5f, 0.5f),
				new Vector3 (-0.5f, -0.5f, 0),
				new Vector3 (-0.5f, 0, -0.5f),
				new Vector3 (0, -0.5f, -0.5f),
				new Vector3 (0.5f, 0.5f, 0),
				new Vector3 (0.5f, 0, 0.5f),
				new Vector3 (0, 0.5f, 0.5f),
			};
			trapezoid.Mesh.triangles = new int[3 * 8];
		}
	}

	void OnSceneGUI ()
	{
		for (int i = 0; i < trapezoid.Mesh.vertexCount; i++) {
			EditorGUI.BeginChangeCheck ();
			var point = Handles.DoPositionHandle (trapezoid.transform.TransformPoint (trapezoid.Mesh.vertices [i]), trapezoid.transform.rotation);
			if (EditorGUI.EndChangeCheck ()) {
				trapezoid.Mesh.vertices [i] = trapezoid.transform.InverseTransformPoint (point); 
				EditorUtility.SetDirty (trapezoid);
			}
		}
	}
}
