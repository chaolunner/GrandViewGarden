using System.Collections.Generic;
using UnityEditorInternal;
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor (typeof(Cylinder))]
public class CylinderDrawer : Editor
{
	public Cylinder Cylinder;
	public bool ShowPosition = true;
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
		Cylinder = target as Cylinder;
		if (Cylinder.Mesh == null) {
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
		if (Cylinder.Mesh == null || Cylinder.Vertices == null || Cylinder.MultipleVertices.Count != Cylinder.Mesh.vertices.Length) {
			CreateMesh ();
		}

		var icon = EditorGUIUtility.IconContent ("EditCollider");
		icon.tooltip = "Edit Mesh Vertices\n\n- Hold Ctrl after clicking control handle to Multi-point editing";
		var bounds = new Bounds (Cylinder.transform.TransformPoint (Cylinder.Mesh.bounds.center), Cylinder.Mesh.bounds.size);
		EditMode.DoEditModeInspectorModeButton (EditMode.SceneViewEditMode.Collider, "Edit Point", icon, bounds, this);
		if (!editingHexahedron) {
			selectedIndexs.Clear ();
		}

		EditorGUI.BeginChangeCheck ();
		var direction = EditorGUILayout.Popup ("Direction", Cylinder.direction, directions);
		var uvsOption = EditorGUILayout.Popup ("UVs", Cylinder.uvsOption, uvsOptions);
		var segments = EditorGUILayout.IntSlider ("Segments", Cylinder.segments, 3, 60);
		var sectionRadius = Cylinder.sectionRadius;
		var bottomRadius = Cylinder.bottomRadius;

		var controlRect = EditorGUILayout.GetControlRect ();
		controlRect = EditorGUI.PrefixLabel (controlRect, new GUIContent ("Radius"));
		controlRect.xMax -= 13;
		if (Cylinder.radiusOption == 0) {
			bottomRadius = EditorGUI.FloatField (controlRect, Cylinder.bottomRadius);
			if (sectionRadius != bottomRadius) {
				sectionRadius = bottomRadius;
				GUI.changed = true;
			}
		} else {
			sectionRadius = EditorGUI.FloatField (new Rect (controlRect.x, controlRect.y, 0.5f * controlRect.width - 13, controlRect.height), Cylinder.sectionRadius);
			bottomRadius = EditorGUI.FloatField (new Rect (controlRect.x + 0.5f * controlRect.width + 13, controlRect.y, 0.5f * controlRect.width - 13, controlRect.height), Cylinder.bottomRadius);
		}
		
		var popupRect = GUILayoutUtility.GetLastRect ();
		popupRect.xMin = popupRect.xMax - 13;
		if (EditorGUI.DropdownButton (popupRect, new GUIContent (""), FocusType.Passive, "ShurikenDropdown")) {
			var menu = new GenericMenu ();
			for (int i = 0; i < radiusOptions.Length; i++) {
				var index = i;
				menu.AddItem (radiusOptions [index], Cylinder.radiusOption == index, () => {
					Cylinder.radiusOption = index;
				});
			}
			menu.ShowAsContext ();
			Event.current.Use ();
		}

		var thickness = EditorGUILayout.FloatField ("Thickness", Cylinder.thickness);
		if (EditorGUI.EndChangeCheck ()) {
			Undo.RecordObject (Cylinder, "Change Cylinder");
			Cylinder.direction = direction;
			Cylinder.segments = segments;
			Cylinder.sectionRadius = sectionRadius;
			if (bottomRadius > 0) {
				Cylinder.bottomRadius = bottomRadius;
			}
			Cylinder.uvsOption = uvsOption;
			Cylinder.thickness = thickness;
			Cylinder.ResetMesh ();
			EditorUtility.SetDirty (Cylinder);
		}

		ShowPosition = EditorGUILayout.Foldout (ShowPosition, "Edit Point", true);
		EditorGUI.indentLevel++;
		if (ShowPosition) {
			for (int i = 0; i < Cylinder.Vertices.Length; i++) {
				EditorGUI.BeginChangeCheck ();
				var point = EditorGUILayout.Vector3Field ("Point " + i, Cylinder.Vertices [i]);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RecordObject (Cylinder, "Move Point");
					Cylinder.Vertices [i] = point;
					var vertices = Cylinder.Mesh.vertices;
					foreach (var index in Cylinder.MultipleVertices[i]) {
						vertices [index] = Cylinder.Vertices [i];
					}
					Cylinder.Mesh.vertices = vertices;
					EditorUtility.SetDirty (Cylinder);
				}
			}
		}
		EditorGUI.indentLevel--;
	}

	void OnSceneGUI ()
	{
		if (Cylinder.Mesh == null || Cylinder.Vertices == null || Cylinder.MultipleVertices.Count != Cylinder.Mesh.vertices.Length) {
			CreateMesh ();
		}

		if (meshCollider == null) {
			CreateCollider ();
		}

		if (editingHexahedron) {
			for (int i = 0; i < Cylinder.Vertices.Length; i++) {
				var pos = Cylinder.transform.TransformPoint (Cylinder.Vertices [i]);
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
				if (Handles.Button (pos, Cylinder.transform.rotation, size, pickSize, Handles.DotHandleCap)) {
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
				var pos = Cylinder.transform.TransformPoint (Cylinder.Vertices [selectedIndex]);
				Handles.Label (pos, "Point " + selectedIndex);
				EditorGUI.BeginChangeCheck ();
				var point = Handles.DoPositionHandle (pos, Cylinder.transform.rotation);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RecordObject (Cylinder, "Move Point");
					var delta = Cylinder.transform.InverseTransformPoint (point) - Cylinder.Vertices [selectedIndex];
					foreach (var id in selectedIndexs) {
						var vertices = Cylinder.Mesh.vertices;
						Cylinder.Vertices [id] += delta;
						foreach (var index in Cylinder.MultipleVertices[id]) {
							vertices [index] = Cylinder.Vertices [id];
						}
						Cylinder.Mesh.vertices = vertices;

						meshCollider.sharedMesh = Cylinder.Mesh;
					}
					EditorUtility.SetDirty (Cylinder);
				}
			}
		}
	}

	void CreateMesh ()
	{
		Cylinder.CreateMesh ();

		CreateCollider ();
	}

	void CreateCollider ()
	{
		if (PrefabUtility.GetPrefabType (target) == PrefabType.Prefab) {
			return;
		}

		meshCollider = Cylinder.GetComponent<MeshCollider> ();
		if (meshCollider == null) {
			meshCollider = Cylinder.gameObject.AddComponent<MeshCollider> ();
			meshCollider.hideFlags = HideFlags.HideAndDontSave;
		}
		meshCollider.sharedMesh = Cylinder.Mesh;
	}
}
