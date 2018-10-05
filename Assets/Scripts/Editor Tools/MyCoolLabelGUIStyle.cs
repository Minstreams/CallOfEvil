#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Label GUI Style", menuName = "系统配置文件/Label GUI Style")]
public class MyCoolLabelGUIStyle : ScriptableObject {
    public float enlargeFactor; //标签放大系数
    public float maxDistanceRange; //缩放范围最大距离
    public Vector2 edges;
    public Color backgroundColor;
    public Color backgroundColorSelected;
    public Color backgroundColorParentSelected;
    public GUIStyle style;

    public static System.Action updateStyle;

    [ContextMenu("UpdateStyle")]
    private void UpdateStyle()
    {
        if (updateStyle != null) updateStyle();
    }
}
#endif