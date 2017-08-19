using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(FreeDriving))]
public class CarDrivingExchange : Editor {
	public FreeDriving freeDriving;	

	void OnEnable () 
	{
		freeDriving = target as FreeDriving;
	}
	public override void OnInspectorGUI ()
	{
		if (GUILayout.Button ("Do Exchange") && freeDriving.DoDriving.CanExecute.Value) {			
			freeDriving.DoDriving.Execute ();
		}
	}
}
