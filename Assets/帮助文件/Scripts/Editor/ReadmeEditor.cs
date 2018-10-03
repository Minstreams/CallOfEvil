using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Reflection;

[CustomEditor(typeof(Readme))]
[InitializeOnLoad]
public class ReadmeEditor : Editor
{
    static string kShowedReadmeSessionStateName = "ReadmeEditor.showedHelp";

    static bool editMode = false;

    static ReadmeEditor()
    {
        EditorApplication.delayCall += SelectReadmeAutomatically;
    }

    /// <summary>
    /// 确保只调用一次SelectReadme
    /// </summary>
    static void SelectReadmeAutomatically()
    {
        if (!SessionState.GetBool(kShowedReadmeSessionStateName, false))
        {
            SelectReadme();
            SessionState.SetBool(kShowedReadmeSessionStateName, true);
        }
    }

    [MenuItem("开发者工具/编辑ReadMe %e")]
    [AddComponentMenu("编辑ReadMe")]
    static void SwitchEditMode()
    {
        editMode = !editMode;
    }
    //static void LoadLayout()
    //{
    //    var assembly = typeof(EditorApplication).Assembly;
    //    var windowLayoutType = assembly.GetType("UnityEditor.WindowLayout", true);
    //    var method = windowLayoutType.GetMethod("LoadWindowLayout", BindingFlags.Public | BindingFlags.Static);
    //    method.Invoke(null, new object[] { Path.Combine(Application.dataPath, "Layout.wlt"), false });
    //}

    [MenuItem("自制工具/Help _F12")]
    static void SelectReadme()
    {
        var ids = AssetDatabase.FindAssets("Help t:Readme");
        if (ids.Length == 0)
        {
            Debug.Log("找不到帮助文件！");
            return;
        }

        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ids[0]));
    }

    protected override void OnHeaderGUI()
    {
        var readme = (Readme)target;

        if (readme.tIcon)
        {
            float iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth / 3f - 20f, 128f, readme.tIcon.width);
            float iconHeight = readme.tIcon.height * iconWidth / readme.tIcon.width;
            GUILayout.BeginHorizontal(Styles.header, GUILayout.Height(iconHeight));
            GUILayout.Label(readme.tIcon, Styles.texture, GUILayout.Width(iconWidth), GUILayout.Height(iconHeight));
            GUILayout.Label(readme.title, Styles.title);
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal(Styles.header, GUILayout.Height(Styles.title.CalcHeight(new GUIContent(readme.title), EditorGUIUtility.currentViewWidth)));
            GUILayout.Label(readme.title, Styles.title);
            GUILayout.EndHorizontal();
        }
    }

    public override void OnInspectorGUI()
    {
        var readme = (Readme)target;
        if (editMode)
        {
            DrawDefaultInspector();
            if (GUILayout.Button("结束编辑", Styles.button))
            {
                SwitchEditMode();
            }
            DrawHeader();
        }

        GUILayout.BeginVertical(Styles.body);

        if (readme.sections == null || readme.sections.Length == 0)
        {
            if (!editMode && GUILayout.Button("编辑内容", Styles.button))
            {
                SwitchEditMode();
            }
            GUILayout.EndVertical();
            return;
        }

        foreach (var section in readme.sections)
        {
            GUILayout.BeginVertical(Styles.section);
            if (!string.IsNullOrEmpty(section.heading))
            {
                GUILayout.Label(section.heading, Styles.heading);
            }
            if (section.picture)
            {
                float picWidth = Mathf.Min(EditorGUIUtility.currentViewWidth - 40f, section.picture.width);
                float picHeight = section.picture.height * picWidth / section.picture.width;
                GUILayout.Label(section.picture, GUILayout.Width(picWidth), GUILayout.Height(picHeight));
            }
            if (!string.IsNullOrEmpty(section.text))
            {
                GUILayout.Label(section.text, Styles.text);
            }
            if (!string.IsNullOrEmpty(section.linkText))
            {
                if (LinkLabel(new GUIContent(section.linkText)))
                {
                    if (!string.IsNullOrEmpty(section.url))
                    {
                        Application.OpenURL(section.url);
                    }
                    if (section.selectedObject)
                    {
                        Selection.activeObject = section.selectedObject;
                    }
                }
            }
            GUILayout.EndVertical();
        }

        GUILayout.EndVertical();
    }

    ReadmeGUIStyles Styles
    {
        get
        {
            if (((Readme)target).styleOverride != null) return ((Readme)target).styleOverride;
            if (m_Styles == null) m_Styles = (ReadmeGUIStyles)AssetDatabase.LoadMainAssetAtPath("Assets/帮助文件/Styles/Default.asset");
            return m_Styles;
        }
    }
    ReadmeGUIStyles m_Styles = null;

    bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
    {
        var position = GUILayoutUtility.GetRect(label, Styles.link, options);

        Handles.BeginGUI();
        Handles.color = Styles.link.normal.textColor;
        Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
        Handles.color = Color.white;
        Handles.EndGUI();

        EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

        return GUI.Button(position, label, Styles.link);
    }
}

