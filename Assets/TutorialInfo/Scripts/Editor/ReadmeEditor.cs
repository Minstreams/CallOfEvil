using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Reflection;

[CustomEditor(typeof(Readme))]
[InitializeOnLoad]
public class ReadmeEditor : Editor
{

    //static string kShowedReadmeSessionStateName = "ReadmeEditor.showedHelp";

    static float kSpace = 16f;

    static bool editMode = false;

    static ReadmeEditor()
    {
        EditorApplication.delayCall += SelectReadme;
    }


    //static void SelectReadmeAutomatically()
    //{
    //    if (!SessionState.GetBool(kShowedReadmeSessionStateName, false))
    //    {
    //        SelectReadme();
    //        SessionState.SetBool(kShowedReadmeSessionStateName, true);
    //    }
    //}

    [MenuItem("Developer Tools/编辑ReadMe %#e")]
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

    [MenuItem("自制脚本/Help")]
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
        Init();

        var iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth / 3f - 20f, 128f);

        GUILayout.BeginHorizontal("In BigTitle");
        {
            if (readme.icon)
            {
                GUILayout.Label(readme.icon, GUILayout.Width(iconWidth), GUILayout.Height(iconWidth));
            }
            GUILayout.Label(readme.title, TitleStyle);
        }
        GUILayout.EndHorizontal();
    }

    public override void OnInspectorGUI()
    {
        if (editMode)
        {
            DrawDefaultInspector();
            var fontsize = GUI.skin.button.fontSize;
            GUI.skin.button.fontSize = 30;
            if (GUILayout.Button(new GUIContent("结束编辑"), GUILayout.Height(60)))
            {
                SwitchEditMode();
            }
            GUI.skin.button.fontSize = fontsize;
            DrawHeader();
        }
        var readme = (Readme)target;
        Init();

        if (readme.sections == null || readme.sections.Length == 0)
        {
            var fontsize = GUI.skin.button.fontSize;
            GUI.skin.button.fontSize = 30;
            if (!editMode && GUILayout.Button(new GUIContent("编辑内容"), GUILayout.Height(60)))
            {
                SwitchEditMode();
            }
            GUI.skin.button.fontSize = fontsize;
            return;
        }

        foreach (var section in readme.sections)
        {
            if (!string.IsNullOrEmpty(section.heading))
            {
                GUILayout.Label(section.heading, HeadingStyle);
            }
            if (section.picture)
            {
                float picWidth = Mathf.Min(EditorGUIUtility.currentViewWidth - 40f, section.picture.width);
                GUILayout.Label(section.picture, GUILayout.Width(picWidth));

            }
            if (!string.IsNullOrEmpty(section.text))
            {
                GUILayout.Label(section.text, BodyStyle);
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
            GUILayout.Space(kSpace);
        }
    }


    bool m_Initialized;

    GUIStyle LinkStyle { get { return m_LinkStyle; } }
    [SerializeField] GUIStyle m_LinkStyle;

    GUIStyle TitleStyle { get { return m_TitleStyle; } }
    [SerializeField] GUIStyle m_TitleStyle;

    GUIStyle HeadingStyle { get { return m_HeadingStyle; } }
    [SerializeField] GUIStyle m_HeadingStyle;

    GUIStyle BodyStyle { get { return m_BodyStyle; } }
    [SerializeField] GUIStyle m_BodyStyle;

    void Init()
    {
        if (m_Initialized)
            return;
        //初始化各个Style
        m_BodyStyle = new GUIStyle(EditorStyles.label);
        m_BodyStyle.wordWrap = true;
        m_BodyStyle.fontSize = 14;

        m_TitleStyle = new GUIStyle(m_BodyStyle);
        m_TitleStyle.fontSize = 26;

        m_HeadingStyle = new GUIStyle(m_BodyStyle);
        m_HeadingStyle.fontSize = 18;

        m_LinkStyle = new GUIStyle(m_BodyStyle);
        m_LinkStyle.wordWrap = false;
        // Match selection color which works nicely for both light and dark skins
        m_LinkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
        m_LinkStyle.stretchWidth = false;

        m_Initialized = true;
    }

    bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
    {
        var position = GUILayoutUtility.GetRect(label, LinkStyle, options);

        Handles.BeginGUI();
        Handles.color = LinkStyle.normal.textColor;
        Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
        Handles.color = Color.white;
        Handles.EndGUI();

        EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

        return GUI.Button(position, label, LinkStyle);
    }
}

