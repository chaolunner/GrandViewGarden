using UnityEditor;
using UnityEngine;

[CustomEditor (typeof(Hexahedron))]
public class HexahedronDrawer : Editor
{
	public Hexahedron hexahedron;

	void OnEnable ()
	{
		hexahedron = target as Hexahedron;
		if (hexahedron.Mesh == null || hexahedron.Mesh.vertexCount != 8) {
			hexahedron.Mesh = new Mesh ();
			hexahedron.Mesh.vertices = new Vector3[] {
				0.5f * new Vector3 (-1, -1, -1),
				0.5f * new Vector3 (-1, 1, -1),
				0.5f * new Vector3 (-1, -1, 1),
				0.5f * new Vector3 (-1, 1, 1),
				0.5f * new Vector3 (1, -1, -1),
				0.5f * new Vector3 (1, 1, -1),
				0.5f * new Vector3 (1, -1, 1),
				0.5f * new Vector3 (1, 1, 1),
			};
			hexahedron.Mesh.triangles = new int[] {
				2, 1, 0,
				2, 3, 1,
				6, 3, 2,
				6, 7, 3,
				4, 7, 6,
				4, 5, 7,
				0, 5, 4,
				0, 1, 5,
				1, 3, 7,
				7, 5, 1,
				6, 2, 0,
				0, 4, 6,
			};
			hexahedron.Mesh.normals = new Vector3[] {
				Vector3.up,
				Vector3.up,
				Vector3.up,
				Vector3.up,
				Vector3.up,
				Vector3.up,
				Vector3.up,
				Vector3.up,
			};
			hexahedron.Mesh.RecalculateBounds ();
			hexahedron.Mesh.RecalculateTangents ();
		}
		Undo.undoRedoPerformed += UndoRedoPerformed;
	}

	void OnDisable ()
	{
		Undo.undoRedoPerformed -= UndoRedoPerformed;
	}

	public override void OnInspectorGUI ()
	{
		if (hexahedron.Mesh == null) {
			return;
		}
		var isDirty = false;
		var vertices = hexahedron.Mesh.vertices;
		for (int i = 0; i < vertices.Length; i++) {
			EditorGUI.BeginChangeCheck ();
			var point = EditorGUILayout.Vector3Field ("Point " + i, hexahedron.transform.TransformPoint (vertices [i]));
			if (EditorGUI.EndChangeCheck ()) {
				Undo.RecordObject (hexahedron.Mesh, "Move Point");
				vertices [i] = hexahedron.transform.InverseTransformPoint (point); 
				EditorUtility.SetDirty (hexahedron);
				isDirty = true;
			}
		}
		if (isDirty) {
			hexahedron.Mesh.vertices = vertices;
		}
	}

	void OnSceneGUI ()
	{
		if (hexahedron.Mesh == null) {
			return;
		}
		var isDirty = false;
		var vertices = hexahedron.Mesh.vertices;
		for (int i = 0; i < vertices.Length; i++) {
			var pos = hexahedron.transform.TransformPoint (vertices [i]);
			Handles.Label (pos, "Point " + i);
			EditorGUI.BeginChangeCheck ();
			var point = Handles.DoPositionHandle (pos, hexahedron.transform.rotation);
			if (EditorGUI.EndChangeCheck ()) {
				Undo.RecordObject (hexahedron.Mesh, "Move Point");
				vertices [i] = hexahedron.transform.InverseTransformPoint (point); 
				EditorUtility.SetDirty (hexahedron);
				isDirty = true;
			}
		}
		if (isDirty) {
			hexahedron.Mesh.vertices = vertices;
		}
	}

	void UndoRedoPerformed ()
	{
		hexahedron.Mesh.vertices = hexahedron.Mesh.vertices;
	}
}
