using UnityEditor;
using UnityEngine;

[CustomEditor (typeof(ColorComponent))]
public class ColorComponentDrawer : Editor
{
	public bool foldout;
	public ColorComponent colorComponent;

	void OnEnable ()
	{
		colorComponent = target as ColorComponent;
	}

	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector ();

		foldout = EditorGUILayout.Foldout (foldout, "Color Components");
		if (foldout) {
			EditorGUI.indentLevel++;
			foreach (var child in colorComponent.ColorCollection) {
				EditorGUILayout.ObjectField (child, typeof(ColorComponent), true);
			}
			EditorGUI.indentLevel--;
		}
	}
}
