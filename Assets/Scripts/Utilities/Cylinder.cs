using System.Collections.Generic;
using UnityEngine;

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
