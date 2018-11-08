using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class SerializableDictionaryDrawer : PropertyDrawer
{
    private bool enabled;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int count = property.FindPropertyRelative("count").intValue;
        return enabled ? (count == 0 ? 2 : count + 1) * EditorGUIUtility.singleLineHeight : EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        enabled = EditorGUI.Foldout(position, enabled, label);

        if (enabled)
        {
            int count = property.FindPropertyRelative("count").intValue;
            SerializedProperty keyListProperty = property.FindPropertyRelative("keyList");
            SerializedProperty valueListProperty = property.FindPropertyRelative("valueList");
            if (count == 0)
            {
                position.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(position, "Empty!");
            }
            for (int i = 0; i < count; i++)
            {
                position.y += EditorGUIUtility.singleLineHeight;
                var valuePosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(GetKeyString(keyListProperty.GetArrayElementAtIndex(i))));
                EditorGUI.LabelField(valuePosition, GetValueString(valueListProperty.GetArrayElementAtIndex(i)));
            }
        }

        EditorGUI.EndProperty();
    }

    //针对不同Type，类似ToString，返回想显示在面板上的内容
    public abstract string GetKeyString(SerializedProperty property);
    public abstract string GetValueString(SerializedProperty property);
}


[CustomPropertyDrawer(typeof(PrefabDictionary))]
public class PrefabDictionaryDrawer : SerializableDictionaryDrawer
{
    public override string GetKeyString(SerializedProperty property)
    {
        Object prefab = property.objectReferenceValue;
        return prefab == null ? "missing" : prefab.name;
    }

    public override string GetValueString(SerializedProperty property)
    {
        return property.stringValue;
    }
}
