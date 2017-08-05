using UnityEditor;
using UnityEngine;

[CustomEditor (typeof(Cylinder))]
public class CylinderDrawer : Editor
{
	public Cylinder cylinder;
	string[] directions = new string[] { "X-Axis", "Y-Axis", "Z-Axis" };
	int[] optionValues = { 0, 1, 2 };

	void OnEnable ()
	{
		cylinder = target as Cylinder;
		if (cylinder.Mesh == null) {
			CreateCylinder ();
		}
		Undo.undoRedoPerformed += CreateCylinder;
	}

	void OnDisable ()
	{
		Undo.undoRedoPerformed -= CreateCylinder;
	}

	public override void OnInspectorGUI ()
	{
		if (cylinder.Mesh == null) {
			CreateCylinder ();
		}

		cylinder.direction = EditorGUILayout.IntPopup ("Direction", cylinder.direction, directions, optionValues);
		cylinder.segments = EditorGUILayout.IntSlider ("Segments", cylinder.segments, 1, 60);
		cylinder.radius = EditorGUILayout.FloatField ("Radius", cylinder.radius);
		cylinder.thickness = EditorGUILayout.FloatField ("Thickness", cylinder.thickness);
	}

	Vector3[] GetVertices ()
	{
		var circleLength = cylinder.segments + 2;
		var length = 4 * circleLength;
		var angle = 360 / cylinder.segments;
		var up = cylinder.direction == 0 ? Vector3.right : cylinder.direction == 1 ? Vector3.up : Vector3.forward;
		var right = cylinder.direction == 0 ? -Vector3.forward : Vector3.right;

		var vertices = new Vector3[length];
		Undo.RecordObject (cylinder, "Reset To Default");

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

	void CreateCylinder ()
	{
		cylinder.Mesh = new Mesh ();
		cylinder.Mesh.vertices = GetVertices ();
		cylinder.Mesh.uv = GetUVsMap ();
		cylinder.Mesh.triangles = GetTriangles ();
		cylinder.Mesh.RecalculateBounds ();
		cylinder.Mesh.RecalculateNormals ();
	}
}
