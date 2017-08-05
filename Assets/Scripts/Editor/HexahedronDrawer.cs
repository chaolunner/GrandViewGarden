using System.Collections.Generic;
using UnityEditorInternal;
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor (typeof(Hexahedron))]
public class HexahedronDrawer : Editor
{
	public Hexahedron hexahedron;
	public bool showPosition = true;
	public MeshCollider meshCollider;
	public List<int> selectedIndexs = new List<int> ();

	public bool editingHexahedron {
		get {
			return EditMode.editMode == EditMode.SceneViewEditMode.Collider && EditMode.IsOwner (this);
		}
	}

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
		if (hexahedron.Mesh == null || hexahedron.Vertices == null || hexahedron.MultipleVertices.Count != hexahedron.Mesh.vertices.Length) {
			CreateMesh ();
		}

		var icon = EditorGUIUtility.IconContent ("EditCollider");
		icon.tooltip = "Edit Mesh Vertices\n\n- Hold Ctrl after clicking control handle to Multi-point editing";
		var bounds = new Bounds (hexahedron.transform.TransformPoint (hexahedron.Mesh.bounds.center), hexahedron.Mesh.bounds.size);
		EditMode.DoEditModeInspectorModeButton (EditMode.SceneViewEditMode.Collider, "Edit Point", icon, bounds, this);
		if (!editingHexahedron) {
			selectedIndexs.Clear ();
		}

		showPosition = EditorGUILayout.Foldout (showPosition, "Edit Point", true);
		EditorGUI.indentLevel++;
		if (showPosition) {
			for (int i = 0; i < hexahedron.Vertices.Length; i++) {
				EditorGUI.BeginChangeCheck ();
				var point = EditorGUILayout.Vector3Field ("Point " + i, hexahedron.Vertices [i]);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RecordObject (hexahedron, "Move Point");
					hexahedron.Vertices [i] = point;
					var vertices = hexahedron.Mesh.vertices;
					foreach (var index in hexahedron.MultipleVertices[i]) {
						vertices [index] = hexahedron.Vertices [i];
					}
					hexahedron.Mesh.vertices = vertices;
					EditorUtility.SetDirty (hexahedron);
				}
			}
		}
		EditorGUI.indentLevel--;

		var content = EditorGUIUtility.IconContent ("Refresh");
		content.text = " Reset To Default";
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.Space ();
		if (GUILayout.Button (content)) {
			ResetMesh ();
		}
		EditorGUILayout.EndHorizontal ();
	}

	void OnSceneGUI ()
	{
		if (hexahedron.Mesh == null || hexahedron.Vertices == null || hexahedron.MultipleVertices.Count != hexahedron.Mesh.vertices.Length) {
			CreateMesh ();
		}

		if (meshCollider == null) {
			CreateCollider ();
		}

		if (editingHexahedron) {
			for (int i = 0; i < hexahedron.Vertices.Length; i++) {
				var pos = hexahedron.transform.TransformPoint (hexahedron.Vertices [i]);
				var size = 0.025f * HandleUtility.GetHandleSize (pos);
				var pickSize = 2 * size;
				var color = Color.white;

				if (selectedIndexs.Contains (i)) {
					color = Color.yellow;
					size = 1.5f * size;
				}
				
				var pivot = SceneView.currentDrawingSceneView.pivot - SceneView.currentDrawingSceneView.size * SceneView.currentDrawingSceneView.camera.transform.forward;
				var hit = new RaycastHit ();
				var ray = new Ray (pivot, Vector3.Normalize (pos - pivot));
				if (Physics.Raycast (ray, out hit, Vector3.Distance (pos, pivot))) {
					if (Mathf.Abs (hit.distance - Vector3.Distance (pos, pivot)) > 0.0001f) {
						color = new Color (color.r, color.g, color.b, 0.25f * color.a);
					}
				}

				Handles.color = color;
				if (Handles.Button (pos, hexahedron.transform.rotation, size, pickSize, Handles.DotHandleCap)) {
					if (!Event.current.control) {
						selectedIndexs.Clear ();
					}
					if (selectedIndexs.Contains (i)) {
						selectedIndexs.Remove (i);
					} else {
						selectedIndexs.Add (i);
					}
				}
			}
				
			if (selectedIndexs.Count > 0) {
				var selectedIndex = selectedIndexs.Last ();
				var pos = hexahedron.transform.TransformPoint (hexahedron.Vertices [selectedIndex]);
				Handles.Label (pos, "Point " + selectedIndex);
				EditorGUI.BeginChangeCheck ();
				var point = Handles.DoPositionHandle (pos, hexahedron.transform.rotation);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RecordObject (hexahedron, "Move Point");
					var delta = hexahedron.transform.InverseTransformPoint (point) - hexahedron.Vertices [selectedIndex];
					foreach (var id in selectedIndexs) {
						var vertices = hexahedron.Mesh.vertices;
						hexahedron.Vertices [id] += delta;
						foreach (var index in hexahedron.MultipleVertices[id]) {
							vertices [index] = hexahedron.Vertices [id];
						}
						hexahedron.Mesh.vertices = vertices;

						meshCollider.sharedMesh = hexahedron.Mesh;
					}
					EditorUtility.SetDirty (hexahedron);
				}
			}
		}
	}

	Vector3[] GetVertices ()
	{
		var vertex_0 = 0.5f * new Vector3 (-hexahedron.DefaultSize.x, -hexahedron.DefaultSize.y, hexahedron.DefaultSize.z);
		var vertex_1 = 0.5f * new Vector3 (hexahedron.DefaultSize.x, -hexahedron.DefaultSize.y, hexahedron.DefaultSize.z);
		var vertex_2 = 0.5f * new Vector3 (hexahedron.DefaultSize.x, -hexahedron.DefaultSize.y, -hexahedron.DefaultSize.z);
		var vertex_3 = 0.5f * new Vector3 (-hexahedron.DefaultSize.x, -hexahedron.DefaultSize.y, -hexahedron.DefaultSize.z);    
		var vertex_4 = 0.5f * new Vector3 (-hexahedron.DefaultSize.x, hexahedron.DefaultSize.y, hexahedron.DefaultSize.z);
		var vertex_5 = 0.5f * new Vector3 (hexahedron.DefaultSize.x, hexahedron.DefaultSize.y, hexahedron.DefaultSize.z);
		var vertex_6 = 0.5f * new Vector3 (hexahedron.DefaultSize.x, hexahedron.DefaultSize.y, -hexahedron.DefaultSize.z);
		var vertex_7 = 0.5f * new Vector3 (-hexahedron.DefaultSize.x, hexahedron.DefaultSize.y, -hexahedron.DefaultSize.z);
		var vertices = new Vector3[] {
			vertex_0, vertex_1, vertex_2, vertex_3,
			vertex_7, vertex_4, vertex_0, vertex_3,
			vertex_4, vertex_5, vertex_1, vertex_0,
			vertex_3, vertex_2, vertex_6, vertex_7,
			vertex_5, vertex_6, vertex_2, vertex_1,
			vertex_7, vertex_6, vertex_5, vertex_4,
		};
		return vertices;
	}

	Vector2[] GetUVsMap ()
	{
		var uvs = new Vector2[24];
		for (int i = 0; i < 6; i++) {
			uvs [4 * i + 0] = new Vector2 (1f, 1f);
			uvs [4 * i + 1] = new Vector2 (0f, 1f);
			uvs [4 * i + 2] = new Vector2 (0f, 0f);
			uvs [4 * i + 3] = new Vector2 (1f, 0f);
		}
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

		if (hexahedron.Vertices == null) {
			Undo.RecordObject (hexahedron, "Reset To Default");
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

		hexahedron.MultipleVertices.Clear ();
		hexahedron.MultipleVertices.Add (0, new int[] { 00, 06, 11, });
		hexahedron.MultipleVertices.Add (1, new int[] { 01, 10, 19, });
		hexahedron.MultipleVertices.Add (2, new int[] { 02, 13, 18, });
		hexahedron.MultipleVertices.Add (3, new int[] { 03, 07, 12, });
		hexahedron.MultipleVertices.Add (4, new int[] { 05, 08, 23, });
		hexahedron.MultipleVertices.Add (5, new int[] { 09, 16, 22, });
		hexahedron.MultipleVertices.Add (6, new int[] { 14, 17, 21, });
		hexahedron.MultipleVertices.Add (7, new int[] { 04, 15, 20, });

		var vertices = new Vector3[hexahedron.Mesh.vertices.Length];
		foreach (var pair in hexahedron.MultipleVertices) {
			foreach (var index in pair.Value) {
				vertices [index] = hexahedron.Vertices [pair.Key];
			}
		}
		hexahedron.Mesh.vertices = vertices;

		CreateCollider ();
	}

	void ResetMesh ()
	{
		hexahedron.Vertices = null;
		CreateMesh ();
	}

	void CreateCollider ()
	{
		meshCollider = hexahedron.GetComponent<MeshCollider> ();
		if (meshCollider == null) {
			meshCollider = hexahedron.gameObject.AddComponent<MeshCollider> ();
			meshCollider.hideFlags = HideFlags.HideAndDontSave;
		}
		meshCollider.sharedMesh = hexahedron.Mesh;
	}
}
