using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer (typeof(MinMaxRangeAttribute))]
public class MinMaxRangeDrawer : PropertyDrawer
{
	private float min;
	private float max;

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
	{
		var range = attribute as MinMaxRangeAttribute;

		SerializedProperty prop_min = property.FindPropertyRelative ("min");
		SerializedProperty prop_max = property.FindPropertyRelative ("max");

		if (prop_min != null && prop_max != null) {

			min = GetValue (prop_min);
			max = GetValue (prop_max);

			label = EditorGUI.BeginProperty (position, label, property);  
			EditorGUI.BeginChangeCheck ();
			 
			Rect contentPosition = EditorGUI.PrefixLabel (position, label);
			float f1, f2;
			if (float.TryParse (EditorGUI.TextField (new Rect (contentPosition.position, new Vector2 (0.2f * contentPosition.width, contentPosition.height)), min.ToString ("F2")), out f1)) {
				min = f1;
			}
			EditorGUI.MinMaxSlider (new Rect (contentPosition.position + new Vector2 (0.2f * contentPosition.width, 0), new Vector2 (0.6f * contentPosition.width, contentPosition.height)), ref min, ref max, range.minLimit, range.maxLimit);
			if (float.TryParse (EditorGUI.TextField (new Rect (contentPosition.position + new Vector2 (0.8f * contentPosition.width, 0), new Vector2 (0.2f * contentPosition.width, contentPosition.height)), max.ToString ("F2")), out f2)) {
				max = f2;
			}

			if (EditorGUI.EndChangeCheck ()) {
				SetValue (prop_min, min);
				SetValue (prop_max, max);
			}
			EditorGUI.EndProperty ();
		}
	}

	private float GetValue (SerializedProperty property)
	{
		if (property.propertyType == SerializedPropertyType.Integer) {
			return property.intValue;
		} else if (property.propertyType == SerializedPropertyType.Float) {
			return property.floatValue;
		}
		return 0;
	}

	private void SetValue (SerializedProperty property, float value)
	{
		if (property.propertyType == SerializedPropertyType.Integer) {
			property.intValue = (int)value;
		} else if (property.propertyType == SerializedPropertyType.Float) {
			property.floatValue = value;
		}
	}
}
