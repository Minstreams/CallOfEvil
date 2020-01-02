#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 酷酷的标签
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("自制工具/标签")]
public class MyCoolLabel : MonoBehaviour
{
    [Space(10)]
    [Header("酷酷的标签名称")]
    public string text = "新标签";
    [Range(0.1f, 2)]
    public float size = 1f;
    [ContextMenu("ResetSize")]
    private void ResetSize() { size = 1f; }

    [MenuItem("自制工具/给选中的游戏物体添加标签 %L")]
    static void AddToActiveGameObject()
    {
        GameObject[] g = Selection.gameObjects;
        if (g == null || g.Length == 0)
        {
            Debug.LogAssertion("没有选中游戏物体");
            return;
        }
        foreach (GameObject gg in g)
        {
            Undo.AddComponent<MyCoolLabel>(gg);
        }
    }





    [System.NonSerialized]
    public Rect rect;

    private static MyCoolLabelGUIStyle style;
    public static MyCoolLabelGUIStyle Style { get { if (style == null) { style = (MyCoolLabelGUIStyle)EditorGUIUtility.Load("Label GUI Style.asset"); } return style; } }

    private GUIStyle gStyle = null;
    public GUIStyle GStyle { get { if (gStyle == null) gStyle = new GUIStyle(Style.style); return gStyle; } }

    private void OnEnable()
    {
        MyCoolLabelGUIStyle.updateStyle += UpdateGStyle;
    }

    [ContextMenu("UpdateStyle")]
    private void UpdateGStyle()
    {
        gStyle = new GUIStyle(Style.style);
    }
    public void DrawLabel(bool selected, bool parentSelected = false)
    {
        if (text == "") return;
        Vector3 point = transform.position;

        //矫正位置
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite) point += Vector3.up * sr.sprite.bounds.extents.y * transform.localScale.y;
        var mr = GetComponent<MeshRenderer>();
        if (mr != null) point += mr.GetComponent<MeshFilter>().sharedMesh.bounds.max.y * Vector3.up;

        point += Vector3.up * 0.12f;

        //距离
        float dist = Vector3.Dot(point - Camera.current.transform.position, Camera.current.transform.forward);
        if (dist < 0) return;

        //字体参数
        float t = 1 - Mathf.Clamp01(dist / Style.maxDistanceRange);
        float fontFactor = Mathf.Lerp(1f, Style.enlargeFactor, t * t) * size;
        GStyle.fontSize = (int)(Style.style.fontSize * fontFactor);

        Vector2 strSize = GStyle.CalcSize(new GUIContent(text)) + Style.edges;
        Vector2 scPos = HandleUtility.WorldToGUIPoint(point);

        float alpha = selected ? 1f : Mathf.Clamp01(-Camera.current.transform.forward.y + 0.35f);
        //alpha = Mathf.Lerp(-0.2f, 1f, 1 - alpha * alpha);

        //绘制
        Handles.BeginGUI();

        rect.size = strSize;
        rect.center = scPos - new Vector2(0, strSize.y / 2);

        Color bc = selected ? Style.backgroundColorSelected : (parentSelected ? Style.backgroundColorParentSelected : Style.backgroundColor);
        bc.a = alpha;
        GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, true, 0, bc, 0, 0);

        if (!selected || Selection.gameObjects.Length > 1)
        {
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, alpha);
            GUI.Label(rect, text, GStyle);
            GUI.color = Color.white;
        }

        Handles.EndGUI();
    }
}
#endif