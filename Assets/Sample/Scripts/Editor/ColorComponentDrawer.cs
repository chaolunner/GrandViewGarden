using UnityEditor;
using UnityEngine;

[CustomEditor (typeof(ColorComponent))]
public class ColorComponentDrawer : Editor
{
	public bool Foldout;
	public ColorComponent ColorComponent;

	void OnEnable ()
	{
		ColorComponent = target as ColorComponent;
	}

	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector ();

		Foldout = EditorGUILayout.Foldout (Foldout, "Color Components");
		if (Foldout) {
			EditorGUI.indentLevel++;
			foreach (var child in ColorComponent.ColorCollection) {
				EditorGUILayout.ObjectField (child, typeof(ColorComponent), true);
			}
			EditorGUI.indentLevel--;
		}
	}
}
