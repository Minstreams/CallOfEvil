using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CircleLoopManager))]
public class CircleLoopManagerEditor : Editor
{
    private void OnSceneGUI()
    {
        CircleLoopManager manager = target as CircleLoopManager;

        Handles.BeginGUI();
        GUILayout.Label(manager.currentAngle.ToString());

        Ray mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        float ty = mouseRay.origin.y / mouseRay.direction.y;
        Vector3 hitPos = new Vector3(mouseRay.origin.x - mouseRay.direction.x * ty, 0, mouseRay.origin.z - mouseRay.direction.z * ty);

        Handles.EndGUI();

        manager.SetCurrentAngle(CircleLoopManager.GetAngle(hitPos));

        Handles.SphereHandleCap(0, hitPos, Quaternion.identity, 0.5f, EventType.Repaint);

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
            }
            manager.AddUnitSorted(unit);
        }
    }

    [MenuItem("测试工具/DeleteUnit %Y")]
    public static void DeleteUnit()
    {
        CircleLoopManager manager = CircleLoopManager.Instance;
        GameObject[] gs = Selection.gameObjects;

        foreach (GameObject g in gs)
        {
            CLUnit unit = g.GetComponent<CLUnit>();
            if (unit != null)
            {
                manager._DeleteUnit(unit);
            }
        }
    }
}
