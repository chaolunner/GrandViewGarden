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
	string[] uvsOptions = new string[] { "Simple", "Sliced" };

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

		EditorGUI.BeginChangeCheck ();
		var uvsOption = EditorGUILayout.Popup ("UVs", hexahedron.uvsOption, uvsOptions);
		var size = EditorGUILayout.Vector3Field ("Size", hexahedron.DefaultSize);
		if (EditorGUI.EndChangeCheck ()) {
			Undo.RecordObject (hexahedron, "Change Hexahedron");
			hexahedron.uvsOption = uvsOption;
			hexahedron.DefaultSize = size;
		}

		var content = EditorGUIUtility.IconContent ("Refresh");
		content.text = " Reset To Default";
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.Space ();
		if (GUILayout.Button (content)) {
			hexahedron.ResetMesh ();
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

	void CreateMesh ()
	{
		Undo.RecordObject (hexahedron, "Reset To Default");
		hexahedron.CreateMesh ();

		CreateCollider ();
	}

	void CreateCollider ()
	{
		if (PrefabUtility.GetPrefabType (target) == PrefabType.Prefab) {
			return;
		}

		meshCollider = hexahedron.GetComponent<MeshCollider> ();
		if (meshCollider == null) {
			meshCollider = hexahedron.gameObject.AddComponent<MeshCollider> ();
			meshCollider.hideFlags = HideFlags.HideAndDontSave;
		}
		meshCollider.sharedMesh = hexahedron.Mesh;
	}
}
