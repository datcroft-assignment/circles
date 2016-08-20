using System;
using UnityEngine;
using UnityEditor;

[Serializable]
public class FromToFloat
{
    public float From;
    public float To;

    public float Lerp(float k)
    {
        return Mathf.Lerp(From, To, k);
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(FromToFloat))]
public class FromToFloatDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        var fromRect = new Rect(80, position.y, 40, position.height);
        var toRect = new Rect(140, position.y, 40, position.height);
        EditorGUI.PropertyField(fromRect, property.FindPropertyRelative("From"), GUIContent.none);
        EditorGUI.PropertyField(toRect, property.FindPropertyRelative("To"), GUIContent.none);
        EditorGUI.EndProperty();
    }
}
#endif
