using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EditorSystem
{
    public class MapInspector : EditorWindow
    {
        //引用
        private static MapManagerStyle Style { get { return MapManager.Style; } }

        //字段
        private string debugMessage;
        private bool debugGUILog;

        private GameObject target;
        private Editor selectedEditor;



        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
        }


        private void DebugMessageUpdate()
        {
            debugMessage =
                "【Debug Message】\n" +
                "Current Object: " + Selection.activeObject + "\n" +
                "Current GameObject: " + Selection.activeGameObject + "\n" +
                "GameObject Count: " + Selection.gameObjects.Length + "\n" +
                "GameObject Type: " + (Selection.activeGameObject == null ? 0 : PrefabUtility.GetPrefabType(Selection.activeGameObject)) + "\n" +
                "GameObject Prefab: " + (Selection.activeGameObject == null ? null : PrefabUtility.GetPrefabObject(Selection.activeGameObject)) + "\n" +
                "GameObject Parent Prefab: " + (Selection.activeGameObject == null ? null : PrefabUtility.FindValidUploadPrefabInstanceRoot(Selection.activeGameObject));
        }

        /// <summary>
        /// 选择项检查结果
        /// </summary>
        private enum SelectionCheckResult
        {
            None,    //没有选中任何
            Multiple,   //选中多项
            Prefab,     //选中Prefab
            Instance,   //选中实例
            Invalid
        }
        private SelectionCheckResult result = SelectionCheckResult.None;

        /// <summary>
        /// 检查选择项
        /// </summary>
        private SelectionCheckResult SelectionCheck()
        {
            if (Selection.gameObjects.Length > 1)
            {
                target = null;
                selectedEditor = null;
                return SelectionCheckResult.Multiple;
            }
            target = Selection.activeGameObject;
            if (target == null)
            {
                selectedEditor = null;
                return SelectionCheckResult.None;
            }

            selectedEditor = Editor.CreateEditor(target);
            switch (PrefabUtility.GetPrefabType(target))
            {
                case PrefabType.Prefab:
                    return SelectionCheckResult.Prefab;
                case PrefabType.PrefabInstance:
                    return SelectionCheckResult.Instance;
                default:
                    return SelectionCheckResult.Invalid;
            }
        }

        private void OnSelectionChanged()
        {
            //这里封装一层是为了避免过多SelectionCheck
            result = SelectionCheck();
            Repaint();
        }

        private void OnGUI()
        {
            debugGUILog = EditorGUILayout.Toggle("Debug GUI Log", debugGUILog);
            if (debugGUILog) Debug.Log("OnGUI():EventType - " + Event.current.type);

            switch (result)
            {
                case SelectionCheckResult.Prefab:
                    //选中一个Prefab   
                    if (selectedEditor != null) selectedEditor.DrawPreview(GUILayoutUtility.GetRect(10, 200, GUILayout.ExpandWidth(true)));
                    break;
                case SelectionCheckResult.Instance:
                    //选中的是一个实例
                    GameObject rootObj = PrefabUtility.FindValidUploadPrefabInstanceRoot(Selection.activeGameObject);
                    if (Selection.activeGameObject != rootObj) Selection.activeGameObject = rootObj;

                    if (selectedEditor != null) selectedEditor.DrawPreview(GUILayoutUtility.GetRect(10, 200, GUILayout.ExpandWidth(true)));
                    GUILayout.Label(target.name);

                    break;

                case SelectionCheckResult.Invalid:
                    //选中没有Prefab的物体
                    GUILayout.Label("这个东西不在地图系统里。");
                    break;

            }
            DebugMessageUpdate();
            GUILayout.Label(debugMessage, Style.debugMessageStyle);
        }

    }
}
