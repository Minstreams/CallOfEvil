using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GameSystem;

namespace EditorSystem
{
    /// <summary>
    /// 用于代替Inspector的窗口
    /// </summary>
    public class MapInspector : EditorWindow
    {
        //引用
        private static EditorMatrixPrefs Prefs { get { return EditorMatrix.Prefs; } }

        //获取选择对象---------------------------------------------------
        private GameObject target;
        private GameObject prefab;
        private Editor selectedEditor;
        private MapGroup currentGroup { get { return MapSystem.groupList[MapSystem.currentGroupIndex]; } }


        //选择检查/修正---------------------------------------------------
        public void OnSelectionChanged()
        {
            if (editing)
            {
                editing = false;
                SaveInformation();
                Debug.Log("刚才的信息编辑已经自动保存！");
            }

            //这里封装一层是为了避免过多SelectionCheck
            result = SelectionCheck();

            information = Prefs.prefabDictionary.ContainsKey(prefab) ? Prefs.prefabDictionary[prefab] : "这个物体还没有介绍，点击编辑添加介绍！";

            Repaint();
        }
        /// <summary>
        /// 检查选择项
        /// </summary>
        private SelectionCheckResult SelectionCheck()
        {
            checkd = false;
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
                    prefab = target;
                    return SelectionCheckResult.Prefab;
                case PrefabType.PrefabInstance:
                    prefab = PrefabUtility.GetCorrespondingObjectFromSource(target) as GameObject;
                    return SelectionCheckResult.Instance;
                default:
                    return SelectionCheckResult.Invalid;
            }
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
        private SelectionCheckResult result = SelectionCheckResult.Invalid;
        private bool checkd;
        /// <summary>
        /// 修正选择
        /// </summary>
        private void AfterSelectionCheck()
        {
            if (checkd) return;
            if (result == SelectionCheckResult.Instance)
            {
                GameObject rootObj = target;
                while (rootObj.transform.parent != null && rootObj.transform.parent.tag != "MapSystem")
                {
                    rootObj = rootObj.transform.parent.gameObject;
                }
                if (Selection.activeGameObject != rootObj)
                    Selection.activeGameObject = rootObj;
            }
            checkd = true;
        }



        //Debug相关------------------------------------------------------
        private string debugMessage = "";
        private bool debugGUILog = false;
        private void DebugMessageUpdate()
        {
            debugMessage =
                "【Debug Message】\n" +
                "Current Object: " + Selection.activeObject + "\n" +
                "Current GameObject: " + Selection.activeGameObject + "\n" +
                "Target: " + target + "\n" +
                "GameObject Count: " + Selection.gameObjects.Length + "\n" +
                "target Type: " + (Selection.activeGameObject == null ? 0 : PrefabUtility.GetPrefabType(Selection.activeGameObject)) + "\n" +
                "target Prefab: " + prefab + "\n" +
                "GameObject Parent Prefab: " + (Selection.activeGameObject == null ? null : PrefabUtility.FindValidUploadPrefabInstanceRoot(Selection.activeGameObject)) + "\n" +
                "\nSceneView Count: " + SceneView.sceneViews.Count + "\n" +
                "currentSceneView: " + SceneView.currentDrawingSceneView + "\n" +
                "lastSceneView: " + SceneView.lastActiveSceneView + "\n";




            foreach (SceneView sv in SceneView.sceneViews)
            {
                debugMessage += "\nScene " + sv.name + ";camera: " + sv.camera.name + ";cPos: " + sv.camera.transform.position;
            }
        }



        //Prefab编辑相关-----------------------------------------------
        private bool editing = false;
        private string information;
        public void SaveInformation()
        {
            if (!Prefs.prefabDictionary.ContainsKey(prefab))
            {
                Prefs.prefabDictionary.Add(prefab, information);
            }
            else
            {
                Prefs.prefabDictionary[prefab] = information;
            }
        }



        //组界面相关---------------------------------------------------
        private MapGroupAsset groupAsset;


        //GUI封装
        public void DrawPrefabInformation()
        {
            if (selectedEditor != null) selectedEditor.DrawPreview(GUILayoutUtility.GetRect(10, 200, GUILayout.ExpandWidth(true)));
            GUILayout.Label(target.name, Prefs.nameStyle);

            if (editing)
            {
                information = EditorGUILayout.TextArea(information, Prefs.informationStyle);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("保存"))
                {
                    SaveInformation();
                    editing = false;
                }
                if (GUILayout.Button("取消"))
                {
                    information = Prefs.prefabDictionary.ContainsKey(prefab) ? Prefs.prefabDictionary[prefab] : "这个物体还没有介绍，点击编辑添加介绍！";
                    editing = false;
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label(information, Prefs.informationStyle);
                if (GUILayout.Button("编辑"))
                {
                    editing = true;
                }
            }
        }




        private void OnEnable()
        {
            Input.imeCompositionMode = IMECompositionMode.On;
            autoRepaintOnSceneChange = true;
            Selection.selectionChanged += OnSelectionChanged;
            MapSystem.OnIndexChanged += Repaint;
        }
        private void OnGUI()
        {
            if (!MapManager.Active)
            {
                GUILayout.Label("地图没打开，地图编辑器未激活！");
                return;
            }

            AfterSelectionCheck();

            switch (result)
            {
                case SelectionCheckResult.None:
                    //没有选中东西
                    //显示当前组信息
                    if (currentGroup == null)
                    {
                        GUILayout.Label("当前组为空！");
                        GUILayout.Label("当前组序号：" + MapSystem.currentGroupIndex);
                        if (GUILayout.Button("新建组")) MapGroupAssetEditor.NewEmptyGroup(MapSystem.currentGroupIndex);
                        groupAsset = EditorGUILayout.ObjectField("To 加载", groupAsset, typeof(MapGroupAsset), false) as MapGroupAsset;
                        if (GUILayout.Button("载入组")) MapGroupAssetEditor.LoadGroup(groupAsset, MapSystem.currentGroupIndex);
                    }
                    else
                    {
                        currentGroup.gameObject.name = currentGroup.groupName = EditorGUILayout.TextField(currentGroup.groupName);
                        GUILayout.Label("当前组序号：" + currentGroup.index);
                        if (GUILayout.Button("保存组")) MapGroupAssetEditor.SaveGroup(currentGroup);
                        if (GUILayout.Button("卸载组")) MapGroupAssetEditor.UnLoadGroup(currentGroup);
                        if (MapGroupAssetEditor.ContainsAsset(currentGroup.groupName) && GUILayout.Button("删除组")) MapGroupAssetEditor.DeleteGroupAsset(MapGroupAssetEditor.GetGroupAssetByName(currentGroup.groupName));
                    }
                    break;

                case SelectionCheckResult.Multiple:
                    //选中多个东西
                    break;

                case SelectionCheckResult.Prefab:
                    //选中一个Prefab
                    DrawPrefabInformation();
                    break;

                case SelectionCheckResult.Instance:
                    //选中的是一个实例
                    DrawPrefabInformation();
                    break;

                case SelectionCheckResult.Invalid:
                    //选中没有Prefab的物体
                    GUILayout.Label("这个东西不在地图系统里。", Prefs.informationStyle);
                    break;
            }

            DebugMessageUpdate();
            GUILayout.Label(debugMessage, Prefs.debugMessageStyle);

            debugGUILog = EditorGUILayout.Toggle("Debug GUI Log", debugGUILog);
            if (debugGUILog) Debug.Log("OnGUI():EventType - " + Event.current.type);
        }




    }
}
