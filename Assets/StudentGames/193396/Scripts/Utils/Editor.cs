using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ReadOnlyAttribute : PropertyAttribute
{
}

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
	{
		string valueStr;

		switch (prop.propertyType)
		{
			case SerializedPropertyType.Integer:
				valueStr = prop.intValue.ToString();
				break;
			case SerializedPropertyType.Boolean:
				valueStr = prop.boolValue.ToString();
				break;
			case SerializedPropertyType.Float:
				valueStr = prop.floatValue.ToString("0.00000");
				break;
			case SerializedPropertyType.String:
				valueStr = prop.stringValue;
				break;
			case SerializedPropertyType.Vector2:
				valueStr = prop.vector2Value.ToString();
				break;
			case SerializedPropertyType.Vector2Int:
				valueStr = prop.vector2IntValue.ToString();
				break;
			case SerializedPropertyType.Vector3:
				valueStr = prop.vector3Value.ToString();
				break;
			case SerializedPropertyType.Vector3Int:
				valueStr = prop.vector3IntValue.ToString();
				break;
			default:
				valueStr = "(not supported)";
				break;
		}

		EditorGUI.LabelField(position, label.text, valueStr);
	}
}

public class FlattenAttribute : PropertyAttribute
{
}

[CustomPropertyDrawer(typeof(FlattenAttribute))]
public class FlattenDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
	{
		foreach (SerializedProperty property in prop)
			EditorGUILayout.PropertyField(property);

	}
}
