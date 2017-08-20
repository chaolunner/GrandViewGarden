using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CombineMeshes : MonoBehaviour
{
	public List<MeshFilter> IgnoreList = new List<MeshFilter> ();
	public Dictionary<string, Material> MaterialDictionary = new Dictionary<string, Material> ();
	public Dictionary<string, List<CombineInstance>> CombineDictionary = new Dictionary<string, List<CombineInstance>> ();

	void Start ()
	{
		var meshFilters = GetComponentsInChildren<MeshFilter> ().Where (x => !x.transform.Equals (transform) && !IgnoreList.Contains (x)).ToArray ();
		var i = 0;

		while (i < meshFilters.Length) {
			var mf = meshFilters [i];
			var mr = mf.GetComponent<MeshRenderer> ();
			var combineInstance = new CombineInstance ();
			var pos = transform.InverseTransformPoint (mf.transform.position);
			var upwords = transform.InverseTransformDirection (mf.transform.up);
			var forward = transform.InverseTransformDirection (mf.transform.forward);
			var q = Quaternion.LookRotation (forward, upwords);
			var s = new Vector3 (mf.transform.lossyScale.x / transform.lossyScale.x, mf.transform.lossyScale.y / transform.lossyScale.y, mf.transform.lossyScale.z / transform.lossyScale.z);
			var matrix = Matrix4x4.TRS (pos, q, s);

			combineInstance.mesh = mf.sharedMesh;
			combineInstance.transform = matrix;

			var materialName = mr.sharedMaterial.name;
			if (!MaterialDictionary.ContainsKey (materialName)) {
				MaterialDictionary.Add (materialName, new Material (mr.sharedMaterial));
			}
			if (CombineDictionary.ContainsKey (materialName)) {
				CombineDictionary [materialName].Add (combineInstance);
			} else {
				CombineDictionary.Add (materialName, new List<CombineInstance> () { combineInstance });
			}

			Destroy (mf.gameObject);

			i++;
		}

		var mats = CombineDictionary.Keys.ToArray ();

		for (int j = 0; j < mats.Length; j++) {
			var combine = CombineDictionary [mats [j]].ToArray ();
			var go = new GameObject (mats [j]);
			var meshFilter = go.AddComponent<MeshFilter> ();
			var meshRenderer = go.AddComponent<MeshRenderer> ();

			go.transform.SetParent (transform, false);
			meshFilter.mesh = new Mesh ();
			meshFilter.mesh.CombineMeshes (combine, true, true);
			meshRenderer.sharedMaterial = new Material (MaterialDictionary [mats [j]]);
		}
	}
}
