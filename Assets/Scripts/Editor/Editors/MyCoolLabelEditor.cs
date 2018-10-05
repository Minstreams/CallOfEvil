using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MyCoolLabel))]
public class MyCoolLabelEditor : Editor
{
    private void OnEnable()
    {
        Input.imeCompositionMode = IMECompositionMode.On;
    }

    private void OnSceneGUI()
    {
        MyCoolLabel label = target as MyCoolLabel;
        Handles.BeginGUI();
        label.text = EditorGUI.TextField(label.rect, label.text, label.GStyle);
        Handles.EndGUI();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        MyCoolLabel label = target as MyCoolLabel;

        GUIStyle style = "In BigTitle";
        float width = EditorGUIUtility.currentViewWidth
            - style.padding.horizontal - style.border.horizontal
            - label.GStyle.border.horizontal
            - 35;
        float height = label.GStyle.CalcHeight(new GUIContent(label.text), width);

        EditorGUILayout.BeginVertical(GUILayout.Height((((int)height) / 80 + 1) * 80));
        EditorGUILayout.BeginVertical(style, GUILayout.Height(height + label.GStyle.border.vertical + style.padding.vertical));

        label.text = EditorGUILayout.TextField(label.text, label.GStyle, GUILayout.Width(width), GUILayout.Height(height));
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();
        EditorGUILayout.LabelField(new GUIContent("标签尺寸"), "in title");
        label.size = EditorGUILayout.Slider(label.size, 0.1f, 2f);
    }

}
