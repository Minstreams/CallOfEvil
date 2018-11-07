using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(MapManagerStyle.PrefabDictionary))]
public class SerializableDictionaryDrawer : PropertyDrawer
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
                Object prefab = keyListProperty.GetArrayElementAtIndex(i).objectReferenceValue;
                string prefabName = prefab == null ? "missing" : prefab.name;
                var valuePosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(prefabName));
                EditorGUI.LabelField(valuePosition, valueListProperty.GetArrayElementAtIndex(i).stringValue);
            }
        }

        // Calculate rects
        //var amountRect = new Rect(position.x, position.y, 30, position.height);
        //var unitRect = new Rect(position.x + 35, position.y, 50, position.height);
        //var nameRect = new Rect(position.x + 90, position.y, position.width - 90, position.height);

        // Draw fields - passs GUIContent.none to each so they are drawn without labels
        //EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("keyList"), GUIContent.none);
        //EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("valueList"), GUIContent.none);



        EditorGUI.EndProperty();
    }
}
