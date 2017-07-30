using UnityEngine;

[ExecuteInEditMode]
[RequireComponent (typeof(MeshFilter))]
public class Hexahedron : MonoBehaviour
{
	protected MeshFilter meshFilter;

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

	void Start ()
	{
		
	}
}
