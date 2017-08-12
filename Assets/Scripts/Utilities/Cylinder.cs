using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
[RequireComponent (typeof(MeshFilter))]
[RequireComponent (typeof(MeshRenderer))]
public class Cylinder : MonoBehaviour
{
	[HideInInspector]
	[Tooltip ("The value can be 0, 1 or 2 corresponding to the X, Y and Z axes, respectively.")]
	public int direction = 1;
	[Range (3, 60)]
	public int segments = 20;
	public int radiusOption;
	public float sectionRadius = 1f;
	public float bottomRadius = 1f;
	public float thickness = 1f;
	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;
	[SerializeField]
	private Vector3[] vertices;
	public Dictionary<int, int[]> MultipleVertices = new Dictionary<int, int[]> ();

	public Mesh Mesh {
		get {
			if (meshFilter == null) {
				meshFilter = GetComponent<MeshFilter> () ?? gameObject.AddComponent<MeshFilter> ();
			}
			return meshFilter.sharedMesh;
		}
		set {
			meshFilter.mesh = value;  
		}
	}

	public MeshRenderer MeshRenderer {
		get {
			if (meshRenderer == null) {
				meshRenderer = GetComponent<MeshRenderer> () ?? gameObject.AddComponent<MeshRenderer> ();
			}
			return meshRenderer;
		}
	}

	public Vector3[] Vertices {
		get {
			return vertices;
		}
		set {
			vertices = value;
		}
	}

	#if UNITY_EDITOR
	void OnWillRenderObject ()
	{
		if (Mesh == null) {
			CreateMesh ();
		}
	}
	#endif

	void OnEnable ()
	{
		MeshRenderer.enabled = true;
	}

	void OnDisable ()
	{
		MeshRenderer.enabled = false;
	}

	void Start ()
	{
		
	}

	Vector3[] GetVertices ()
	{
		var circleLength = segments + 2;
		var length = 4 * circleLength;
		var angle = 360 / segments;
		var up = direction == 0 ? Vector3.right : direction == 1 ? Vector3.up : Vector3.forward;
		var right = direction == 0 ? -Vector3.forward : Vector3.right;
		var vertices = new Vector3[length];

		for (int i = 0; i < 4; i++) {
			for (int j = 0; j < circleLength; j++) {
				if (j == 0) {
					vertices [i * circleLength + j] = (i < 2 ? 0.5f : -0.5f) * thickness * up;
				} else {
					var radius = radiusOption == 1 && i < 2 ? sectionRadius : bottomRadius;
					vertices [i * circleLength + j] = radius * (Quaternion.Euler ((j - 1) * angle * up) * right) + vertices [i * circleLength];
				}
			}
		}
		return vertices;
	}

	int[] GetTriangles ()
	{
		var circleLength = segments + 2;
		var length = 12 * segments;
		var triangles = new int[length];

		for (int i = 0; i < 4; i++) {
			for (int j = 0; j < segments; j++) {
				if (i == 0) {
					triangles [3 * (i * segments + j) + 0] = i * circleLength + j + 1;
					triangles [3 * (i * segments + j) + 1] = i * circleLength + j + 2;
					triangles [3 * (i * segments + j) + 2] = i * circleLength;
				} else if (i == 1) {
					triangles [3 * (i * segments + j) + 0] = i * circleLength + j + 1;
					triangles [3 * (i * segments + j) + 1] = (i + 1) * circleLength + j + 1;
					triangles [3 * (i * segments + j) + 2] = (i + 1) * circleLength + j + 2;
				} else if (i == 2) {
					triangles [3 * (i * segments + j) + 0] = i * circleLength + j + 2;
					triangles [3 * (i * segments + j) + 1] = (i - 1) * circleLength + j + 2;
					triangles [3 * (i * segments + j) + 2] = (i - 1) * circleLength + j + 1;
				} else if (i == 3) {
					triangles [3 * (i * segments + j) + 0] = i * circleLength;
					triangles [3 * (i * segments + j) + 1] = i * circleLength + j + 2;
					triangles [3 * (i * segments + j) + 2] = i * circleLength + j + 1;
				}
			}
		}
		return triangles;
	}

	Vector2[] GetUVsMap ()
	{
		var circleLength = segments + 2;
		var angle = 360 / segments;
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

	public void CreateMesh ()
	{
		Mesh = new Mesh ();
		Mesh.vertices = GetVertices ();
		Mesh.uv = GetUVsMap ();
		Mesh.triangles = GetTriangles ();
		Mesh.RecalculateBounds ();
		Mesh.RecalculateNormals ();

		var vertices = new Vector3[Mesh.vertices.Length];
		var circleLength = segments + 2;
		var length = segments + 1;
		if (Vertices == null || Vertices.Length != 2 * length) {
			Vertices = new Vector3[2 * length];
			for (int i = 0; i < length; i++) {
				Vertices [i] = Mesh.vertices [i];
				Vertices [length + i] = Mesh.vertices [2 * circleLength + i];
			}
		}

		MultipleVertices.Clear ();
		for (int i = 0; i < vertices.Length; i++) {
			var index = i % circleLength;
			var key = (index == length ? 1 : index) + (i / circleLength < 2 ? 0 : length);
			if (MultipleVertices.ContainsKey (key)) {
				var list = MultipleVertices [key].ToList ();
				list.Add (i);
				MultipleVertices [key] = list.ToArray ();
			} else {
				MultipleVertices.Add (key, new int[] { i });
			}
			vertices [i] = Vertices [key];
		}
		Mesh.vertices = vertices;
	}

	public void ResetMesh ()
	{
		Vertices = null;
		CreateMesh ();
	}
}
