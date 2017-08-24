using System.Collections.Generic;
using UnityEditorInternal;
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor (typeof(Hexahedron))]
public class HexahedronDrawer : Editor
{
	public Hexahedron Hexahedron;
	public bool ShowPosition = true;
	public MeshCollider meshCollider;
	public List<int> SelectedIndexs = new List<int> ();
	string[] uvsOptions = new string[] { "Simple", "Sliced" };

	public bool editingHexahedron {
		get {
			return EditMode.editMode == EditMode.SceneViewEditMode.Collider && EditMode.IsOwner (this);
		}
	}

	void OnEnable ()
	{
		Hexahedron = target as Hexahedron;
		if (Hexahedron.Mesh == null || Hexahedron.Mesh.vertexCount != 24) {
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
		if (Hexahedron.Mesh == null || Hexahedron.Vertices == null || Hexahedron.MultipleVertices.Count != Hexahedron.Mesh.vertices.Length) {
			CreateMesh ();
		}

		var icon = EditorGUIUtility.IconContent ("EditCollider");
		icon.tooltip = "Edit Mesh Vertices\n\n- Hold Ctrl after clicking control handle to Multi-point editing";
		var bounds = new Bounds (Hexahedron.transform.TransformPoint (Hexahedron.Mesh.bounds.center), Hexahedron.Mesh.bounds.size);
		EditMode.DoEditModeInspectorModeButton (EditMode.SceneViewEditMode.Collider, "Edit Point", icon, bounds, this);
		if (!editingHexahedron) {
			SelectedIndexs.Clear ();
		}

		ShowPosition = EditorGUILayout.Foldout (ShowPosition, "Edit Point", true);
		EditorGUI.indentLevel++;
		if (ShowPosition) {
			for (int i = 0; i < Hexahedron.Vertices.Length; i++) {
				EditorGUI.BeginChangeCheck ();
				var point = EditorGUILayout.Vector3Field ("Point " + i, Hexahedron.Vertices [i]);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RecordObject (Hexahedron, "Move Point");
					Hexahedron.Vertices [i] = point;
					var vertices = Hexahedron.Mesh.vertices;
					foreach (var index in Hexahedron.MultipleVertices[i]) {
						vertices [index] = Hexahedron.Vertices [i];
					}
					Hexahedron.Mesh.vertices = vertices;
					EditorUtility.SetDirty (Hexahedron);
				}
			}
		}
		EditorGUI.indentLevel--;

		EditorGUI.BeginChangeCheck ();
		var uvsOption = EditorGUILayout.Popup ("UVs", Hexahedron.uvsOption, uvsOptions);
		var size = EditorGUILayout.Vector3Field ("Size", Hexahedron.DefaultSize);
		if (EditorGUI.EndChangeCheck ()) {
			Undo.RecordObject (Hexahedron, "Change Hexahedron");
			Hexahedron.uvsOption = uvsOption;
			Hexahedron.DefaultSize = size;
		}

		var content = EditorGUIUtility.IconContent ("Refresh");
		content.text = " Reset To Default";
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.Space ();
		if (GUILayout.Button (content)) {
			Hexahedron.ResetMesh ();
		}
		EditorGUILayout.EndHorizontal ();
	}

	void OnSceneGUI ()
	{
		if (Hexahedron.Mesh == null || Hexahedron.Vertices == null || Hexahedron.MultipleVertices.Count != Hexahedron.Mesh.vertices.Length) {
			CreateMesh ();
		}

		if (meshCollider == null) {
			CreateCollider ();
		}

		if (editingHexahedron) {
			for (int i = 0; i < Hexahedron.Vertices.Length; i++) {
				var pos = Hexahedron.transform.TransformPoint (Hexahedron.Vertices [i]);
				var size = 0.025f * HandleUtility.GetHandleSize (pos);
				var pickSize = 2 * size;
				var color = Color.white;

				if (SelectedIndexs.Contains (i)) {
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
				if (Handles.Button (pos, Hexahedron.transform.rotation, size, pickSize, Handles.DotHandleCap)) {
					if (!Event.current.control) {
						SelectedIndexs.Clear ();
					}
					if (SelectedIndexs.Contains (i)) {
						SelectedIndexs.Remove (i);
					} else {
						SelectedIndexs.Add (i);
					}
				}
			}
				
			if (SelectedIndexs.Count > 0) {
				var selectedIndex = SelectedIndexs.Last ();
				var pos = Hexahedron.transform.TransformPoint (Hexahedron.Vertices [selectedIndex]);
				Handles.Label (pos, "Point " + selectedIndex);
				EditorGUI.BeginChangeCheck ();
				var point = Handles.DoPositionHandle (pos, Hexahedron.transform.rotation);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RecordObject (Hexahedron, "Move Point");
					var delta = Hexahedron.transform.InverseTransformPoint (point) - Hexahedron.Vertices [selectedIndex];
					foreach (var id in SelectedIndexs) {
						var vertices = Hexahedron.Mesh.vertices;
						Hexahedron.Vertices [id] += delta;
						foreach (var index in Hexahedron.MultipleVertices[id]) {
							vertices [index] = Hexahedron.Vertices [id];
						}
						Hexahedron.Mesh.vertices = vertices;

						meshCollider.sharedMesh = Hexahedron.Mesh;
					}
					EditorUtility.SetDirty (Hexahedron);
				}
			}
		}
	}

	void CreateMesh ()
	{
		Undo.RecordObject (Hexahedron, "Reset To Default");
		Hexahedron.CreateMesh ();

		CreateCollider ();
	}

	void CreateCollider ()
	{
		if (PrefabUtility.GetPrefabType (target) == PrefabType.Prefab) {
			return;
		}

		meshCollider = Hexahedron.GetComponent<MeshCollider> ();
		if (meshCollider == null) {
			meshCollider = Hexahedron.gameObject.AddComponent<MeshCollider> ();
			meshCollider.hideFlags = HideFlags.HideAndDontSave;
		}
		meshCollider.sharedMesh = Hexahedron.Mesh;
	}
}
