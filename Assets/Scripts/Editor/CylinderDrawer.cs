using System.Collections.Generic;
using UnityEditorInternal;
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor (typeof(Cylinder))]
public class CylinderDrawer : Editor
{
	public Cylinder cylinder;
	public bool showPosition = true;
	public MeshCollider meshCollider;
	string[] directions = new string[] { "X-Axis", "Y-Axis", "Z-Axis" };
	int[] optionValues = { 0, 1, 2 };
	List<int> selectedIndexs = new List<int> ();

	public bool editingHexahedron {
		get {
			return EditMode.editMode == EditMode.SceneViewEditMode.Collider && EditMode.IsOwner (this);
		}
	}

	void OnEnable ()
	{
		cylinder = target as Cylinder;
		if (cylinder.Mesh == null) {
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
		if (cylinder.Mesh == null || cylinder.Vertices == null || cylinder.MultipleVertices.Count != cylinder.Mesh.vertices.Length) {
			CreateMesh ();
		}

		var icon = EditorGUIUtility.IconContent ("EditCollider");
		icon.tooltip = "Edit Mesh Vertices\n\n- Hold Ctrl after clicking control handle to Multi-point editing";
		var bounds = new Bounds (cylinder.transform.TransformPoint (cylinder.Mesh.bounds.center), cylinder.Mesh.bounds.size);
		EditMode.DoEditModeInspectorModeButton (EditMode.SceneViewEditMode.Collider, "Edit Point", icon, bounds, this);
		if (!editingHexahedron) {
			selectedIndexs.Clear ();
		}

		EditorGUI.BeginChangeCheck ();
		cylinder.direction = EditorGUILayout.IntPopup ("Direction", cylinder.direction, directions, optionValues);
		cylinder.segments = EditorGUILayout.IntSlider ("Segments", cylinder.segments, 1, 60);
		cylinder.radius = EditorGUILayout.FloatField ("Radius", cylinder.radius);
		cylinder.thickness = EditorGUILayout.FloatField ("Thickness", cylinder.thickness);
		if (EditorGUI.EndChangeCheck ()) {
			Undo.RecordObject (cylinder, "Change Cylinder");
			ResetMesh ();
			EditorUtility.SetDirty (cylinder);
		}

		showPosition = EditorGUILayout.Foldout (showPosition, "Edit Point", true);
		EditorGUI.indentLevel++;
		if (showPosition) {
			for (int i = 0; i < cylinder.Vertices.Length; i++) {
				EditorGUI.BeginChangeCheck ();
				var point = EditorGUILayout.Vector3Field ("Point " + i, cylinder.Vertices [i]);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RecordObject (cylinder, "Move Point");
					cylinder.Vertices [i] = point;
					var vertices = cylinder.Mesh.vertices;
					foreach (var index in cylinder.MultipleVertices[i]) {
						vertices [index] = cylinder.Vertices [i];
					}
					cylinder.Mesh.vertices = vertices;
					EditorUtility.SetDirty (cylinder);
				}
			}
		}
		EditorGUI.indentLevel--;
	}

	void OnSceneGUI ()
	{
		if (cylinder.Mesh == null || cylinder.Vertices == null || cylinder.MultipleVertices.Count != cylinder.Mesh.vertices.Length) {
			CreateMesh ();
		}

		if (meshCollider == null) {
			CreateCollider ();
		}

		if (editingHexahedron) {
			for (int i = 0; i < cylinder.Vertices.Length; i++) {
				var pos = cylinder.transform.TransformPoint (cylinder.Vertices [i]);
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
				if (Handles.Button (pos, cylinder.transform.rotation, size, pickSize, Handles.DotHandleCap)) {
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
				var pos = cylinder.transform.TransformPoint (cylinder.Vertices [selectedIndex]);
				Handles.Label (pos, "Point " + selectedIndex);
				EditorGUI.BeginChangeCheck ();
				var point = Handles.DoPositionHandle (pos, cylinder.transform.rotation);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RecordObject (cylinder, "Move Point");
					var delta = cylinder.transform.InverseTransformPoint (point) - cylinder.Vertices [selectedIndex];
					foreach (var id in selectedIndexs) {
						var vertices = cylinder.Mesh.vertices;
						cylinder.Vertices [id] += delta;
						foreach (var index in cylinder.MultipleVertices[id]) {
							vertices [index] = cylinder.Vertices [id];
						}
						cylinder.Mesh.vertices = vertices;

						meshCollider.sharedMesh = cylinder.Mesh;
					}
					EditorUtility.SetDirty (cylinder);
				}
			}
		}
	}

	Vector3[] GetVertices ()
	{
		var circleLength = cylinder.segments + 2;
		var length = 4 * circleLength;
		var angle = 360 / cylinder.segments;
		var up = cylinder.direction == 0 ? Vector3.right : cylinder.direction == 1 ? Vector3.up : Vector3.forward;
		var right = cylinder.direction == 0 ? -Vector3.forward : Vector3.right;
		var vertices = new Vector3[length];

		for (int i = 0; i < 4; i++) {
			for (int j = 0; j < circleLength; j++) {
				if (j == 0) {
					vertices [i * circleLength + j] = (i < 2 ? 0.5f : -0.5f) * cylinder.thickness * up;
				} else {
					vertices [i * circleLength + j] = cylinder.radius * (Quaternion.Euler ((j - 1) * angle * up) * right) + vertices [i * circleLength];
				}
			}
		}
		return vertices;
	}

	int[] GetTriangles ()
	{
		var circleLength = cylinder.segments + 2;
		var length = 12 * cylinder.segments;
		var triangles = new int[length];

		for (int i = 0; i < 4; i++) {
			for (int j = 0; j < cylinder.segments; j++) {
				if (i == 0) {
					triangles [3 * (i * cylinder.segments + j) + 0] = i * circleLength + j + 1;
					triangles [3 * (i * cylinder.segments + j) + 1] = i * circleLength + j + 2;
					triangles [3 * (i * cylinder.segments + j) + 2] = i * circleLength;
				} else if (i == 1) {
					triangles [3 * (i * cylinder.segments + j) + 0] = i * circleLength + j + 1;
					triangles [3 * (i * cylinder.segments + j) + 1] = (i + 1) * circleLength + j + 1;
					triangles [3 * (i * cylinder.segments + j) + 2] = (i + 1) * circleLength + j + 2;
				} else if (i == 2) {
					triangles [3 * (i * cylinder.segments + j) + 0] = i * circleLength + j + 2;
					triangles [3 * (i * cylinder.segments + j) + 1] = (i - 1) * circleLength + j + 2;
					triangles [3 * (i * cylinder.segments + j) + 2] = (i - 1) * circleLength + j + 1;
				} else if (i == 3) {
					triangles [3 * (i * cylinder.segments + j) + 0] = i * circleLength;
					triangles [3 * (i * cylinder.segments + j) + 1] = i * circleLength + j + 2;
					triangles [3 * (i * cylinder.segments + j) + 2] = i * circleLength + j + 1;
				}
			}
		}
		return triangles;
	}

	Vector2[] GetUVsMap ()
	{
		var circleLength = cylinder.segments + 2;
		var angle = 360 / cylinder.segments;
		var length = 4 * circleLength;
		var uvs = new Vector2[length];

		for (int i = 0; i < 4; i++) {
			for (int j = 0; j < circleLength; j++) {
				if (i == 0) {
					if (j == 0) {
						uvs [i * circleLength + j] = 0.5f * Vector2.one;
					} else {
						var radians = ((j - 1) * angle - 90) * Mathf.Deg2Rad;
						var x = 0.5f * (1 + Mathf.Sin (radians));
						var y = 0.5f * (1 + Mathf.Cos (radians));
						uvs [i * circleLength + j] = new Vector2 (x, y);
					}
				} else if (i == 1) {
					uvs [i * circleLength + j] = new Vector2 (1 - (float)j / circleLength, 1);
				} else if (i == 2) {
					uvs [i * circleLength + j] = new Vector2 (1 - (float)j / circleLength, 0);
				} else {
					if (j == 0) {
						uvs [i * circleLength + j] = 0.5f * Vector2.one;
					} else {
						var radians = ((j - 1) * angle - 90) * Mathf.Deg2Rad;
						var x = 0.5f * (1 + Mathf.Sin (radians));
						var y = 0.5f * (1 + Mathf.Cos (radians));
						uvs [i * circleLength + j] = new Vector2 (1 - x, y);
					}
				}
			}
		}
		return uvs;
	}

	void CreateMesh ()
	{
		cylinder.Mesh = new Mesh ();
		cylinder.Mesh.vertices = GetVertices ();
		cylinder.Mesh.uv = GetUVsMap ();
		cylinder.Mesh.triangles = GetTriangles ();
		cylinder.Mesh.RecalculateBounds ();
		cylinder.Mesh.RecalculateNormals ();

		var vertices = new Vector3[cylinder.Mesh.vertices.Length];
		var circleLength = cylinder.segments + 2;
		var length = cylinder.segments + 1;
		if (cylinder.Vertices == null || cylinder.Vertices.Length != 2 * length) {
			cylinder.Vertices = new Vector3[2 * length];
			for (int i = 0; i < length; i++) {
				cylinder.Vertices [i] = cylinder.Mesh.vertices [i];
				cylinder.Vertices [length + i] = cylinder.Mesh.vertices [2 * circleLength + i];
			}
		}
		
		cylinder.MultipleVertices.Clear ();
		for (int i = 0; i < vertices.Length; i++) {
			var index = i % circleLength;
			var key = (index == length ? 1 : index) + (i / circleLength < 2 ? 0 : length);
			if (cylinder.MultipleVertices.ContainsKey (key)) {
				var list = cylinder.MultipleVertices [key].ToList ();
				list.Add (i);
				cylinder.MultipleVertices [key] = list.ToArray ();
			} else {
				cylinder.MultipleVertices.Add (key, new int[] { i });
			}
			vertices [i] = cylinder.Vertices [key];
		}
		cylinder.Mesh.vertices = vertices;

		CreateCollider ();
	}

	void ResetMesh ()
	{
		cylinder.Vertices = null;
		CreateMesh ();
	}

	void CreateCollider ()
	{
		meshCollider = cylinder.GetComponent<MeshCollider> ();
		if (meshCollider == null) {
			meshCollider = cylinder.gameObject.AddComponent<MeshCollider> ();
			meshCollider.hideFlags = HideFlags.HideAndDontSave;
		}
		meshCollider.sharedMesh = cylinder.Mesh;
	}
}
