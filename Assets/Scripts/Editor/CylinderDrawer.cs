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
	string[] uvsOptions = new string[] { "Simple", "Sliced" };
	GUIContent[] radiusOptions = new GUIContent[] {
		new GUIContent ("Constant"),
		new GUIContent ("Between Two Constants"),
	};
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
		var direction = EditorGUILayout.Popup ("Direction", cylinder.direction, directions);
		var uvsOption = EditorGUILayout.Popup ("UVs", cylinder.uvsOption, uvsOptions);
		var segments = EditorGUILayout.IntSlider ("Segments", cylinder.segments, 3, 60);
		var sectionRadius = cylinder.sectionRadius;
		var bottomRadius = cylinder.bottomRadius;

		var controlRect = EditorGUILayout.GetControlRect ();
		controlRect = EditorGUI.PrefixLabel (controlRect, new GUIContent ("Radius"));
		controlRect.xMax -= 13;
		if (cylinder.radiusOption == 0) {
			bottomRadius = EditorGUI.FloatField (controlRect, cylinder.bottomRadius);
			if (sectionRadius != bottomRadius) {
				sectionRadius = bottomRadius;
				GUI.changed = true;
			}
		} else {
			sectionRadius = EditorGUI.FloatField (new Rect (controlRect.x, controlRect.y, 0.5f * controlRect.width - 13, controlRect.height), cylinder.sectionRadius);
			bottomRadius = EditorGUI.FloatField (new Rect (controlRect.x + 0.5f * controlRect.width + 13, controlRect.y, 0.5f * controlRect.width - 13, controlRect.height), cylinder.bottomRadius);
		}
		
		var popupRect = GUILayoutUtility.GetLastRect ();
		popupRect.xMin = popupRect.xMax - 13;
		if (EditorGUI.DropdownButton (popupRect, new GUIContent (""), FocusType.Passive, "ShurikenDropdown")) {
			var menu = new GenericMenu ();
			for (int i = 0; i < radiusOptions.Length; i++) {
				var index = i;
				menu.AddItem (radiusOptions [index], cylinder.radiusOption == index, () => {
					cylinder.radiusOption = index;
				});
			}
			menu.ShowAsContext ();
			Event.current.Use ();
		}

		var thickness = EditorGUILayout.FloatField ("Thickness", cylinder.thickness);
		if (EditorGUI.EndChangeCheck ()) {
			Undo.RecordObject (cylinder, "Change Cylinder");
			cylinder.direction = direction;
			cylinder.segments = segments;
			cylinder.sectionRadius = sectionRadius;
			if (bottomRadius > 0) {
				cylinder.bottomRadius = bottomRadius;
			}
			cylinder.uvsOption = uvsOption;
			cylinder.thickness = thickness;
			cylinder.ResetMesh ();
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

	void CreateMesh ()
	{
		cylinder.CreateMesh ();

		CreateCollider ();
	}

	void CreateCollider ()
	{
		if (PrefabUtility.GetPrefabType (target) == PrefabType.Prefab) {
			return;
		}

		meshCollider = cylinder.GetComponent<MeshCollider> ();
		if (meshCollider == null) {
			meshCollider = cylinder.gameObject.AddComponent<MeshCollider> ();
			meshCollider.hideFlags = HideFlags.HideAndDontSave;
		}
		meshCollider.sharedMesh = cylinder.Mesh;
	}
}
