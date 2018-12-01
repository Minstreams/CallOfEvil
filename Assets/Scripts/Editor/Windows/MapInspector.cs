using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using GameSystem;

namespace EditorSystem
{
    /// <summary>
    /// 用于代替Inspector的窗口
    /// </summary>
    public class MapInspector : EditorWindow
    {
        //引用，定义
        private static EditorMatrixPrefs Prefs { get { return EditorMatrix.Prefs; } }
        public enum SelectionType
        {
            GameObject,
            Prefab,
            Group,
            GroupAsset,
            Map
        }


        //获取选择对象---------------------------------------------------
        private static Object target = null;
        private static Editor selectedEditor = null;
        private static SelectionType selectionType = SelectionType.Map;

        public static void FocusOn(Object focusTarget, SelectionType type)
        {
            Debug.Log("Focus!" + focusTarget + "【" + type + "】");
            if (target == focusTarget && selectionType == type) return;
            if (selectedEditor != null) DestroyImmediate(selectedEditor);
            target = focusTarget;
            selectionType = type;
            try
            {
                switch (type)
                {
                    case SelectionType.GameObject:
                    case SelectionType.Prefab:
                        selectedEditor = target == null ? null : Editor.CreateEditor(target);
                        break;
                    case SelectionType.Group:
                        selectedEditor = target == null ? null : Editor.CreateEditor((target as MapGroup).gameObject);
                        break;
                    case SelectionType.GroupAsset:
                        selectedEditor = target == null ? null : Editor.CreateEditor(((MapGroupAsset)target).groupPrefab);
                        break;
                    case SelectionType.Map:
                        selectedEditor = null;
                        break;
                }
            }
            catch (System.NullReferenceException)
            {
                Debug.Log("Editor Creating Failed!");
            }
            if (instance != null) instance.Repaint();
            editing = false;
        }



        //Debug/General--------------------------------------------------
        private static MapInspector instance = null;
        private void OnEnable()
        {
            instance = this;
            if (selectedEditor != null) DestroyImmediate(selectedEditor);
            selectedEditor = null;
            Input.imeCompositionMode = IMECompositionMode.On;
            //autoRepaintOnSceneChange = true;
        }
        private string debugMessage = "";
        private void ShowDebugMessage()
        {
            debugMessage =
                "【Debug Message】\n" +
                "Target:" + target + (target == null ? "null" : target.name) + "\n" +
                "SelectedEditor:" + (selectedEditor == null ? "null" : selectedEditor.ToString()) + "\n" +
                "SelectionType:" + selectionType + "\n" +
                "";


            //~
            GUILayout.Label(debugMessage, Prefs.debugMessageStyle);
        }
        private void DrawPreview(bool forceToDraw = false)
        {
            if (selectedEditor == null)
            {
                GUILayout.Label("no preview editor", Prefs.mapAssetBackgroundStyle, GUILayout.Height(Prefs.previewHeight));
            }
            else if (forceToDraw || selectedEditor.HasPreviewGUI())
            {
                selectedEditor.DrawPreview(GUILayoutUtility.GetRect(10, Prefs.previewHeight, GUILayout.ExpandWidth(true)));
            }
            else
            {
                GUILayout.Label("no preview gui", Prefs.mapAssetBackgroundStyle, GUILayout.Height(Prefs.previewHeight));
            }
        }


        //Tools
        private static bool editing = false;

        private void EditableNameField(GameObject obj)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(25));
            {
                if (editing) obj.name = EditorGUILayout.TextField(obj.name, Prefs.nameStyle, GUILayout.Height(25));
                else GUILayout.Label(obj.name, Prefs.nameStyle);
                if (GUILayout.Button(editing ? "完成" : "重命名", GUILayout.Width(60))) { editing = !editing; GUI.FocusControl(""); }
            }
            GUILayout.EndHorizontal();
        }
        private void EditableNameField(MapGroup group)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(25));
            {
                if (editing) group.groupName = EditorGUILayout.TextField(group.groupName, Prefs.nameStyle, GUILayout.Height(25));
                else GUILayout.Label(group.groupName, Prefs.nameStyle);
                if (GUILayout.Button(editing ? "完成" : "重命名", GUILayout.Width(60))) { editing = !editing; GUI.FocusControl(""); }
            }
            GUILayout.EndHorizontal();
        }


        //Sub GUIs-------------------------------------------------------
        private void OnGameObjectGUI()
        {
            if (target == null)
            {
                FocusOn(null, SelectionType.Map);
                return;
            }
            DrawPreview(true);
            GameObject gameObject = target as GameObject;

            EditableNameField(gameObject);

            if (GUILayout.Button("", Prefs.mapAssetBackgroundStyle, GUILayout.ExpandHeight(true))) GUI.FocusControl("");
            GUILayout.BeginHorizontal(Prefs.mapAssetBackgroundStyle, GUILayout.Height(28));
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("放到地面", GUILayout.Width(60))) (target as GameObject).transform.Translate(Vector3.down * (target as GameObject).transform.position.y);
            }
            GUILayout.EndHorizontal();

        }

        private void OnPrefabGUI()
        {
            DrawPreview();
            GameObject prefab = target as GameObject;

            EditableNameField(prefab);


            if (MapAssetManager.ContainsPrefab(prefab))
            {
                //在表列
                MapAssetManager.CurrentInformation = EditorGUILayout.TextArea(MapAssetManager.CurrentInformation, Prefs.informationStyle);

                if (GUILayout.Button("", Prefs.mapAssetBackgroundStyle, GUILayout.ExpandHeight(true))) GUI.FocusControl("");
                GUILayout.BeginHorizontal(Prefs.mapAssetBackgroundStyle, GUILayout.Height(28));
                {
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                //不在表列

                if (GUILayout.Button("", Prefs.mapAssetBackgroundStyle, GUILayout.ExpandHeight(true))) GUI.FocusControl("");
                GUILayout.BeginHorizontal(Prefs.mapAssetBackgroundStyle, GUILayout.Height(28));
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("加入物品表列", GUILayout.Width(120))) { MapAssetManager.AddPrefab(prefab); FocusOn(MapAssetManager.CurrentPrefab, SelectionType.Prefab); }
                }
                GUILayout.EndHorizontal();
            }


        }

        private void OnGroupGUI()
        {
            MapGroup group = target as MapGroup;

            DrawPreview(true);

            if (group == null)
            {
                GUILayout.Label("当前组为空", Prefs.informationStyle);

                if (GUILayout.Button("", Prefs.mapAssetBackgroundStyle, GUILayout.ExpandHeight(true))) GUI.FocusControl("");
                GUILayout.BeginHorizontal(Prefs.mapAssetBackgroundStyle, GUILayout.Height(28));
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("新建组", GUILayout.Width(60))) { MapGroupAssetEditor.NewEmptyGroup(MapSystem.currentGroupIndex); FocusOn(MapSystem.groupList[MapSystem.currentGroupIndex], SelectionType.Group); }
                    if (MapAssetManager.CurrentAsset != null && GUILayout.Button("加载组", GUILayout.Width(60))) { MapGroupAssetEditor.LoadGroup(MapAssetManager.CurrentAsset, MapSystem.currentGroupIndex); FocusOn(MapSystem.groupList[MapSystem.currentGroupIndex], SelectionType.Group); }
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                EditableNameField(group);

                if (GUILayout.Button("组序号：" + group.index, Prefs.mapAssetBackgroundStyle, GUILayout.ExpandHeight(true))) GUI.FocusControl("");
                GUILayout.BeginHorizontal(Prefs.mapAssetBackgroundStyle, GUILayout.Height(28));
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("保存组", GUILayout.Width(60))) { MapGroupAssetEditor.SaveGroup(group); FocusOn(MapSystem.groupList[MapSystem.currentGroupIndex], SelectionType.Group); }
                    if (GUILayout.Button("卸载组", GUILayout.Width(60))) { FocusOn(null, SelectionType.Group); MapGroupAssetEditor.UnLoadGroup(group); }
                    if (MapAssetManager.CurrentAsset != null && GUILayout.Button("加载组", GUILayout.Width(60))) { MapGroupAssetEditor.LoadGroup(MapAssetManager.CurrentAsset, MapSystem.currentGroupIndex); FocusOn(MapSystem.groupList[MapSystem.currentGroupIndex], SelectionType.Group); }
                }
                GUILayout.EndHorizontal();
            }
        }

        private void OnGroupAssetGUI()
        {
            if (target == null)
            {
                FocusOn(null, SelectionType.Map);
                return;
            }
            MapGroupAsset groupAsset = target as MapGroupAsset;

            DrawPreview();
            GUILayout.Label(groupAsset.groupName, Prefs.nameStyle);
            groupAsset.information = EditorGUILayout.TextArea(groupAsset.information, Prefs.informationStyle);
            if (GUILayout.Button("", Prefs.mapAssetBackgroundStyle, GUILayout.ExpandHeight(true))) GUI.FocusControl("");
            GUILayout.BeginHorizontal(Prefs.mapAssetBackgroundStyle, GUILayout.Height(28));
            {
                GUILayout.FlexibleSpace();
                if (!groupAsset.baked && GUILayout.Button("烘焙", GUILayout.Width(60))) MapGroupAssetEditor.BakeGroupAsset(groupAsset);
                if (GUILayout.Button("加载组", GUILayout.Width(60))) { MapGroupAssetEditor.LoadGroup(MapAssetManager.CurrentAsset, MapSystem.currentGroupIndex); FocusOn(MapSystem.groupList[MapSystem.currentGroupIndex], SelectionType.Group); }
            }
            GUILayout.EndHorizontal();
        }

        private void OnMapGUI()
        {
            if (GUILayout.Button("", Prefs.mapAssetBackgroundStyle, GUILayout.ExpandHeight(true))) GUI.FocusControl("");
            GUILayout.BeginHorizontal(Prefs.mapAssetBackgroundStyle, GUILayout.Height(28));
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("重新组织场景", GUILayout.Width(120))) TemporaryTool.RearrangeInvalidMapGroup();
            }
            GUILayout.EndHorizontal();
        }







        private void OnGUI()
        {
            if (!MapManager.Active)
            {
                GUILayout.Label("地图没打开，地图编辑器未激活！");
                ShowDebugMessage();
                return;
            }

            try
            {
                switch (selectionType)
                {
                    case SelectionType.GameObject:
                        OnGameObjectGUI();
                        break;
                    case SelectionType.Prefab:
                        OnPrefabGUI();
                        break;
                    case SelectionType.Group:
                        OnGroupGUI();
                        break;
                    case SelectionType.GroupAsset:
                        OnGroupAssetGUI();
                        break;
                    case SelectionType.Map:
                        OnMapGUI();
                        break;
                }
            }
            catch (System.ArgumentException)
            {
                Debug.Log("Layout Problem from MapInspector, event type :" + Event.current.type);
                return;
            }
            ShowDebugMessage();
        }


    }
}
