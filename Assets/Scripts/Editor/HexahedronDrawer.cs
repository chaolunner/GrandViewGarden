using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof(Hexahedron))]
public class HexahedronDrawer : Editor
{
	public Hexahedron hexahedron;
	public int selectedIndex = -1;
	public bool canEditMesh;

	void OnEnable ()
	{
		hexahedron = target as Hexahedron;
		if (hexahedron.Mesh == null || hexahedron.Mesh.vertexCount != 24) {
			CreateMesh ();
		}
		Undo.undoRedoPerformed += CreateMesh;
	}

	void OnDisable ()
	{
		Undo.undoRedoPerformed -= CreateMesh;
	}

	public override void OnInspectorGUI ()
	{
		if (hexahedron.Mesh == null || hexahedron.MultipleVertices == null) {
			CreateMesh ();
		}

		EditorGUILayout.BeginHorizontal ();
		canEditMesh = GUILayout.Toggle (canEditMesh, "Edit Mesh");
		if (!canEditMesh) {
			selectedIndex = -1;
		}
		if (GUILayout.Button ("Reset To Default")) {
			ResetToDefault ();
		}
		EditorGUILayout.EndHorizontal ();

		for (int i = 0; i < hexahedron.Vertices.Length; i++) {
			EditorGUI.BeginChangeCheck ();
			var point = EditorGUILayout.Vector3Field ("Point " + i, hexahedron.Vertices [i]);
			if (EditorGUI.EndChangeCheck ()) {
				Undo.RecordObject (hexahedron, "Move Point");
				hexahedron.Vertices [i] = point;
				var vertices = hexahedron.Mesh.vertices;
				foreach (var index in hexahedron.MultipleVertices[selectedIndex]) {
					vertices [index] = hexahedron.Vertices [i];
				}
				hexahedron.Mesh.vertices = vertices;
				EditorUtility.SetDirty (hexahedron);
			}
		}
	}

	void OnSceneGUI ()
	{
		if (hexahedron.Mesh == null || hexahedron.MultipleVertices == null) {
			CreateMesh ();
		}

		if (canEditMesh) {
			for (int i = 0; i < hexahedron.Vertices.Length; i++) {
				var pos = hexahedron.transform.TransformPoint (hexahedron.Vertices [i]);
				var size = 0.05f * HandleUtility.GetHandleSize (pos);
				var pickSize = 2 * size;
				if (Handles.Button (pos, hexahedron.transform.rotation, size, pickSize, Handles.DotHandleCap)) {
					selectedIndex = i;
				}
			}

			if (selectedIndex >= 0 && selectedIndex < hexahedron.Vertices.Length) {
				var pos = hexahedron.transform.TransformPoint (hexahedron.Vertices [selectedIndex]);
				Handles.Label (pos, "Point " + selectedIndex);
				EditorGUI.BeginChangeCheck ();
				var point = Handles.DoPositionHandle (pos, hexahedron.transform.rotation);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RecordObject (hexahedron, "Move Point");
					hexahedron.Vertices [selectedIndex] = hexahedron.transform.InverseTransformPoint (point); 
					var vertices = hexahedron.Mesh.vertices;
					foreach (var index in hexahedron.MultipleVertices[selectedIndex]) {
						vertices [index] = hexahedron.Vertices [selectedIndex];
					}
					hexahedron.Mesh.vertices = vertices;
					EditorUtility.SetDirty (hexahedron);
				}
			}
		}
	}

	Vector3[] GetVertices ()
	{
		if (hexahedron.Vertices == null) {
			hexahedron.Vertices = new Vector3[8];
			hexahedron.Vertices [0] = 0.5f * new Vector3 (-hexahedron.DefaultSize.x, -hexahedron.DefaultSize.y, hexahedron.DefaultSize.z);
			hexahedron.Vertices [1] = 0.5f * new Vector3 (hexahedron.DefaultSize.x, -hexahedron.DefaultSize.y, hexahedron.DefaultSize.z);
			hexahedron.Vertices [2] = 0.5f * new Vector3 (hexahedron.DefaultSize.x, -hexahedron.DefaultSize.y, -hexahedron.DefaultSize.z);
			hexahedron.Vertices [3] = 0.5f * new Vector3 (-hexahedron.DefaultSize.x, -hexahedron.DefaultSize.y, -hexahedron.DefaultSize.z);    
			hexahedron.Vertices [4] = 0.5f * new Vector3 (-hexahedron.DefaultSize.x, hexahedron.DefaultSize.y, hexahedron.DefaultSize.z);
			hexahedron.Vertices [5] = 0.5f * new Vector3 (hexahedron.DefaultSize.x, hexahedron.DefaultSize.y, hexahedron.DefaultSize.z);
			hexahedron.Vertices [6] = 0.5f * new Vector3 (hexahedron.DefaultSize.x, hexahedron.DefaultSize.y, -hexahedron.DefaultSize.z);
			hexahedron.Vertices [7] = 0.5f * new Vector3 (-hexahedron.DefaultSize.x, hexahedron.DefaultSize.y, -hexahedron.DefaultSize.z);
		}

		hexahedron.MultipleVertices = new Dictionary<int, int[]> ();
		hexahedron.MultipleVertices.Add (0, new int[] { 00, 06, 11, });
		hexahedron.MultipleVertices.Add (1, new int[] { 01, 10, 19, });
		hexahedron.MultipleVertices.Add (2, new int[] { 02, 13, 18, });
		hexahedron.MultipleVertices.Add (3, new int[] { 03, 07, 12, });
		hexahedron.MultipleVertices.Add (4, new int[] { 05, 08, 23, });
		hexahedron.MultipleVertices.Add (5, new int[] { 09, 16, 22, });
		hexahedron.MultipleVertices.Add (6, new int[] { 14, 17, 21, });
		hexahedron.MultipleVertices.Add (7, new int[] { 04, 15, 20, });

		var vertices = new Vector3[24];
		foreach (var pair in hexahedron.MultipleVertices) {
			foreach (var value in pair.Value) {
				vertices [value] = hexahedron.Vertices [pair.Key];
			}
		}

		return vertices;
	}

	Vector2[] GetUVsMap ()
	{
		var _00_CORDINATES = new Vector2 (0f, 0f);
		var _10_CORDINATES = new Vector2 (1f, 0f);
		var _01_CORDINATES = new Vector2 (0f, 1f);
		var _11_CORDINATES = new Vector2 (1f, 1f);
		var uvs = new Vector2[] {
			// Bottom
			_11_CORDINATES, _01_CORDINATES, _00_CORDINATES, _10_CORDINATES,
			// Left
			_11_CORDINATES, _01_CORDINATES, _00_CORDINATES, _10_CORDINATES,
			// Front
			_11_CORDINATES, _01_CORDINATES, _00_CORDINATES, _10_CORDINATES,
			// Back
			_11_CORDINATES, _01_CORDINATES, _00_CORDINATES, _10_CORDINATES,
			// Right
			_11_CORDINATES, _01_CORDINATES, _00_CORDINATES, _10_CORDINATES,
			// Top
			_11_CORDINATES, _01_CORDINATES, _00_CORDINATES, _10_CORDINATES,
		};
		return uvs;
	}

	int[] GetTriangles ()
	{
		var triangles = new int[36];
		for (int i = 0; i < triangles.Length / 6; i++) {
			triangles [6 * i + 0] = 3 + 4 * i;
			triangles [6 * i + 1] = 1 + 4 * i;
			triangles [6 * i + 2] = 0 + 4 * i;
			triangles [6 * i + 3] = 3 + 4 * i;
			triangles [6 * i + 4] = 2 + 4 * i;
			triangles [6 * i + 5] = 1 + 4 * i;
		}
		return triangles;
	}

	void CreateMesh ()
	{
		hexahedron.Mesh = new Mesh ();
		hexahedron.Mesh.vertices = GetVertices ();
		hexahedron.Mesh.uv = GetUVsMap ();
		hexahedron.Mesh.triangles = GetTriangles ();
		hexahedron.Mesh.RecalculateBounds ();
		hexahedron.Mesh.RecalculateNormals ();
	}

	void ResetToDefault ()
	{
		hexahedron.Vertices = null;
		CreateMesh ();
	}
}
