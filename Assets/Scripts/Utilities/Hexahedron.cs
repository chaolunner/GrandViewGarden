using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent (typeof(MeshFilter))]
[RequireComponent (typeof(MeshRenderer))]
public class Hexahedron : MonoBehaviour
{
	public int uvsOption;
	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;
	private Vector3 defaultSize = new Vector3 (1, 1, 1);
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

	public Vector3 DefaultSize {
		get {
			return defaultSize;
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
		var vertex_0 = 0.5f * new Vector3 (-DefaultSize.x, -DefaultSize.y, DefaultSize.z);
		var vertex_1 = 0.5f * new Vector3 (DefaultSize.x, -DefaultSize.y, DefaultSize.z);
		var vertex_2 = 0.5f * new Vector3 (DefaultSize.x, -DefaultSize.y, -DefaultSize.z);
		var vertex_3 = 0.5f * new Vector3 (-DefaultSize.x, -DefaultSize.y, -DefaultSize.z);    
		var vertex_4 = 0.5f * new Vector3 (-DefaultSize.x, DefaultSize.y, DefaultSize.z);
		var vertex_5 = 0.5f * new Vector3 (DefaultSize.x, DefaultSize.y, DefaultSize.z);
		var vertex_6 = 0.5f * new Vector3 (DefaultSize.x, DefaultSize.y, -DefaultSize.z);
		var vertex_7 = 0.5f * new Vector3 (-DefaultSize.x, DefaultSize.y, -DefaultSize.z);
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

	Vector2[] GetSimpleUVsMap ()
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

	Vector2[] GetSlicedUVsMap ()
	{
		var uvs = new Vector2[24];
		for (int i = 0; i < 6; i++) {
			var j = (float)(i / 2);
			uvs [4 * i + 0] = new Vector2 (0.5f * (1 + i % 2), (1 + j) / 3);
			uvs [4 * i + 1] = new Vector2 (0.5f * (i % 2), (1 + j) / 3);
			uvs [4 * i + 2] = new Vector2 (0.5f * (i % 2), j / 3);
			uvs [4 * i + 3] = new Vector2 (0.5f * (1 + i % 2), j / 3);
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

	public void CreateMesh ()
	{
		Mesh = new Mesh ();
		Mesh.vertices = GetVertices ();
		Mesh.uv = uvsOption == 0 ? GetSimpleUVsMap () : GetSlicedUVsMap ();
		Mesh.triangles = GetTriangles ();
		Mesh.RecalculateBounds ();
		Mesh.RecalculateNormals ();

		if (Vertices == null) {
			Vertices = new Vector3[8];
			Vertices [0] = 0.5f * new Vector3 (-DefaultSize.x, -DefaultSize.y, DefaultSize.z);
			Vertices [1] = 0.5f * new Vector3 (DefaultSize.x, -DefaultSize.y, DefaultSize.z);
			Vertices [2] = 0.5f * new Vector3 (DefaultSize.x, -DefaultSize.y, -DefaultSize.z);
			Vertices [3] = 0.5f * new Vector3 (-DefaultSize.x, -DefaultSize.y, -DefaultSize.z);    
			Vertices [4] = 0.5f * new Vector3 (-DefaultSize.x, DefaultSize.y, DefaultSize.z);
			Vertices [5] = 0.5f * new Vector3 (DefaultSize.x, DefaultSize.y, DefaultSize.z);
			Vertices [6] = 0.5f * new Vector3 (DefaultSize.x, DefaultSize.y, -DefaultSize.z);
			Vertices [7] = 0.5f * new Vector3 (-DefaultSize.x, DefaultSize.y, -DefaultSize.z);
		}

		MultipleVertices.Clear ();
		MultipleVertices.Add (0, new int[] { 00, 06, 11, });
		MultipleVertices.Add (1, new int[] { 01, 10, 19, });
		MultipleVertices.Add (2, new int[] { 02, 13, 18, });
		MultipleVertices.Add (3, new int[] { 03, 07, 12, });
		MultipleVertices.Add (4, new int[] { 05, 08, 23, });
		MultipleVertices.Add (5, new int[] { 09, 16, 22, });
		MultipleVertices.Add (6, new int[] { 14, 17, 21, });
		MultipleVertices.Add (7, new int[] { 04, 15, 20, });

		var vertices = new Vector3[Mesh.vertices.Length];
		foreach (var pair in MultipleVertices) {
			foreach (var index in pair.Value) {
				vertices [index] = Vertices [pair.Key];
			}
		}
		Mesh.vertices = vertices;
	}

	public void ResetMesh ()
	{
		Vertices = null;
		CreateMesh ();
	}
}
