using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CircleLoopManager))]
public class CircleLoopManagerEditor : Editor
{
    /// <summary>
    /// 注视鼠标，否则注视摄像机
    /// </summary>
    public static bool focusMouse = true;

    public override void OnInspectorGUI()
    {
        focusMouse = GUILayout.Toggle(focusMouse, "注视鼠标");
        DrawDefaultInspector();
    }

    private void OnSceneGUI()
    {
        CircleLoopManager manager = target as CircleLoopManager;
        Handles.BeginGUI();
        GUILayout.Label(manager.currentAngle.ToString(), "label");
        Handles.EndGUI();
    }

    [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
    static void OnGizmo(CircleLoopManager manager, GizmoType type)
    {
        //获取注视点并更新
        Vector3 hitPos;
        if (CircleLoopManagerEditor.focusMouse)
        {
            Ray mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            float ty = mouseRay.origin.y / mouseRay.direction.y;
            hitPos = new Vector3(mouseRay.origin.x - mouseRay.direction.x * ty, 0, mouseRay.origin.z - mouseRay.direction.z * ty);
        }
        else hitPos = Camera.current.transform.position;

        manager.SetCurrentAngle(manager.GetAngle(hitPos));
        //Handles.SphereHandleCap(0, hitPos, Quaternion.identity, 0.5f, EventType.Repaint);


        //绘制可见范围
        Handles.color = Color.black;
        Handles.DrawSolidArc(Vector3.zero, Vector3.up, Quaternion.Euler(-Vector3.up * (manager.currentAngle - manager.AngleRadius)) * Vector3.right, 360 - 2 * manager.AngleRadius, 20);
        Handles.color = Color.white;


        //绘制中央信息
        GUIStyle style = new GUIStyle("label");
        style.fontSize = 30;
        style.alignment = TextAnchor.LowerCenter;

        Handles.Label(Vector3.up * 0.5f, manager.CurrentCircle.ToString(), style);\

    }

    /// <summary>
    /// 排序插入当前选中的游戏物体
    /// </summary>
    [MenuItem("测试工具/AddUnit %T")]
    public static void AddUnitSorted()
    {
        CircleLoopManager manager = CircleLoopManager.Instance;
        GameObject[] gs = Selection.gameObjects;

        foreach (GameObject g in gs)
        {
            CLUnit unit = g.GetComponent<CLUnit>();
            if (unit == null)
            {
                unit = g.AddComponent<CLUnit>();
                unit.SetActive(true);
            }
            manager.AddUnitSorted(unit);
        }
    }

    [MenuItem("测试工具/DeleteUnit %U")]
    public static void DeleteUnit()
    {
        CircleLoopManager manager = CircleLoopManager.Instance;
        GameObject[] gs = Selection.gameObjects;

        foreach (GameObject g in gs)
        {
            CLUnit unit = g.GetComponent<CLUnit>();
            if (unit != null)
            {
                g.SetActive(true);
                manager._DeleteUnit(unit);
            }
        }
    }
}
