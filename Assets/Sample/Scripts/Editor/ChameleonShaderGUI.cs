using UnityEngine;
using UnityEditor;
using System.Linq;

public class ChameleonShaderGUI : ShaderGUI
{
	override public void OnGUI (MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		var targetMat = materialEditor.target as Material;

		EditorGUI.BeginChangeCheck ();
		var controlRect = EditorGUILayout.GetControlRect ();
		var rect = new Rect (controlRect.position, new Vector2 (18, 18));
		var mainTexture = EditorGUI.ObjectField (rect, targetMat.GetTexture ("_MainTexture"), typeof(Texture), false) as Texture;
		controlRect.xMin += 30;
		controlRect = EditorGUI.PrefixLabel (controlRect, new GUIContent ("Main Texture"));
		var mainColor = EditorGUI.ColorField (new Rect (controlRect.position, new Vector2 (50, controlRect.height)), targetMat.GetColor ("_MainColor"));

		controlRect = EditorGUILayout.GetControlRect ();
		rect = new Rect (controlRect.position, new Vector2 (18, 18));
		var baseTexture = EditorGUI.ObjectField (rect, targetMat.GetTexture ("_BaseTexture"), typeof(Texture), false) as Texture;
		controlRect.xMin += 30;
		controlRect = EditorGUI.PrefixLabel (controlRect, new GUIContent ("Base Texture"));
		var baseColor = EditorGUI.ColorField (new Rect (controlRect.position, new Vector2 (50, controlRect.height)), targetMat.GetColor ("_BaseColor"));

		if (EditorGUI.EndChangeCheck ()) {
			targetMat.SetTexture ("_MainTexture", mainTexture);
			targetMat.SetColor ("_MainColor", mainColor);
			targetMat.SetTexture ("_BaseTexture", baseTexture);
			targetMat.SetColor ("_BaseColor", baseColor);
			EditorUtility.SetDirty (targetMat);
		}

		var showProperties = properties.ToList ();
		showProperties.RemoveRange (0, 4);
		base.OnGUI (materialEditor, showProperties.ToArray ());
	}
}
