using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CreateAssetMenu(fileName = "New Readme GUI Styles", menuName = "帮助文件/Readme GUI 方案")]
public class ReadmeGUIStyles : ScriptableObject
{
    [ContextMenuItem("Set", "Setheader")] public GUIStyle header;
    [ContextMenuItem("Set", "Setbody")] public GUIStyle body;
    [ContextMenuItem("Set", "Seticon")] public GUIStyle icon;
    [ContextMenuItem("Set", "Settitle")] public GUIStyle title;
    [ContextMenuItem("Set", "Setsection")] public GUIStyle section;
    [ContextMenuItem("Set", "Setheading")] public GUIStyle heading;
    [ContextMenuItem("Set", "Settext")] public GUIStyle text;
    [ContextMenuItem("Set", "Setlink")] public GUIStyle link;
    [ContextMenuItem("Set", "Setbutton")] public GUIStyle button;

    [ContextMenu("SetAll")]
    void SetAll()
    {
        SetStyle(ref header);
        SetStyle(ref body);
        SetStyle(ref icon);
        SetStyle(ref title);
        SetStyle(ref section);
        SetStyle(ref heading);
        SetStyle(ref text);
        SetStyle(ref link);
        SetStyle(ref button);
    }

    void SetStyle(ref GUIStyle style)
    {
        GUISkin skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);//获取到内置的Skin	
        GUIStyle gs = skin.FindStyle(style.name);
        Debug.Log(gs);
        if (gs != null)
        {
            style = new GUIStyle(gs);
        }
        else
        {
            style = new GUIStyle();
        }
    }

    void Setheader() { SetStyle(ref header); }
    void Setbody() { SetStyle(ref body); }
    void Seticon() { SetStyle(ref icon); }
    void Settitle() { SetStyle(ref title); }
    void Setsection() { SetStyle(ref section); }
    void Setheading() { SetStyle(ref heading); }
    void Settext() { SetStyle(ref text); }
    void Setlink() { SetStyle(ref link); }
    void Setbutton() { SetStyle(ref button); }
}
#endif