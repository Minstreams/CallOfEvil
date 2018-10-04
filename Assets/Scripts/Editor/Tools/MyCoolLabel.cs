using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 酷酷的标签
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("自制工具/标签")]
public class MyCoolLabel : MonoBehaviour {
    [Space(10)]
    [Header("酷酷的标签名称")]
    public string text = "";
    [Range(0.1f, 2)]
    public float size = 1f;
    private static GUIStyle style;
    private static GUIStyle Style
    {
        get
        {
            if (style == null)
            {
                style = new GUIStyle(EditorStyles.largeLabel);
                style.alignment = TextAnchor.MiddleCenter;
                style.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
                style.fontSize = 32;
            }
            return style;
        }

    }
    void OnDrawGizmos()
    {
        if (text == "") return;
        Vector3 point = transform.position;
        if(GetComponent<SpriteRenderer>()!=null)point = transform.position + Vector3.up * GetComponent<SpriteRenderer>().sprite.bounds.extents.y * transform.localScale.y;
        float dist = (Camera.current.transform.position - point).magnitude;

        float fontSize = Mathf.Lerp(64, 12, dist / 10f) * size;

        Style.fontSize = (int)fontSize;

        Vector3 wPos = point + Camera.current.transform.up * dist * 0.07f;

        Vector3 scPos = Camera.current.WorldToScreenPoint(wPos);
        if (scPos.z <= 0)
        {
            return;
        }

        float alpha = Mathf.Clamp(-Camera.current.transform.forward.y, 0f, 1f);
        alpha = 1f - ((1f - alpha) * (1f - alpha));

        alpha = Mathf.Lerp(-0.2f, 1f, alpha);

        Handles.BeginGUI();

        scPos.y = Screen.height - scPos.y; // Flip Y


        Vector2 strSize = Style.CalcSize(new GUIContent(text));

        Rect rect = new Rect(0f, 0f, strSize.x + 6, strSize.y + 4);
        rect.center = scPos - Vector3.up * rect.height * 0.5f;
        GUI.color = new Color(0f, 0f, 0f, 0.8f * alpha);
        GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
        GUI.color = Color.white;
        GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, alpha);
        GUI.Label(rect, text, Style);
        GUI.color = Color.white;

        Handles.EndGUI();
    }
}
