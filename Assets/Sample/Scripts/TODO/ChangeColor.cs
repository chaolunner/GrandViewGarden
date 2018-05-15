using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
//	public Material red;
//	public Material yellow;
//
	void Start ()
	{
		
	}

	void Update ()
	{
		var renderer = GetComponent<Collider> ().GetComponent<MeshRenderer> ();
		Debug.Log (renderer.materials);		
	}

	void OnTriggerstay (Collider collider)
	{
		
	}
}
