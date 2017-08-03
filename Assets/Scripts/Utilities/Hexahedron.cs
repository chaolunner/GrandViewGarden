using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(MeshFilter))]
[RequireComponent (typeof(MeshRenderer))]
public class Hexahedron : MonoBehaviour
{
	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;
	private Vector3 defaultSize = new Vector3 (1, 1, 1);
	[SerializeField] private Vector3[] vertices;
	public Dictionary<int, int[]> MultipleVertices;

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
}
