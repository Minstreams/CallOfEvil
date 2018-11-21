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
        private static Object target;
        private static Editor selectedEditor;
        private static SelectionType selectionType = SelectionType.Map;

        public static void FocusOn(Object focusTarget, SelectionType type)
        {
            //Debug.Log("Focus!");
            if (target == focusTarget && selectionType == type) return;
            target = focusTarget;
            selectionType = type;
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
                    selectedEditor = Editor.CreateEditor(((MapGroupAsset)target).groupPrefab);
                    break;
                case SelectionType.Map:
                    selectedEditor = null;
                    break;
            }

            if (instance != null) instance.Repaint();
            editing = false;
        }



        //Debug/General--------------------------------------------------
        private static MapInspector instance = null;
        private void OnEnable()
        {
            instance = this;
            selectedEditor = null;
            Input.imeCompositionMode = IMECompositionMode.On;
            autoRepaintOnSceneChange = true;
        }
        private string debugMessage = "";
        private void ShowDebugMessage()
        {
            debugMessage =
                "【Debug Message】\n" +
                "Target:" + target + (target == null ? "" : target.name) + "\n" +
                "SelectedEditor:" + selectedEditor + "\n" +
                "SelectionType:" + selectionType + "\n" +
                "";


            //~
            GUILayout.Label(debugMessage, Prefs.debugMessageStyle);
        }
        private void DrawPreview()
        {
            if (selectedEditor != null && selectedEditor.HasPreviewGUI()) selectedEditor.DrawPreview(GUILayoutUtility.GetRect(10, 200, GUILayout.ExpandWidth(true)));
        }


        //Tools
        private static bool editing = false;
        private string EditableNameField(string name)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(25));
            {
                if (editing) name = EditorGUILayout.TextField(name, Prefs.nameStyle, GUILayout.Height(25));
                else GUILayout.Label(name, Prefs.nameStyle);
                if (GUILayout.Button(editing ? "完成" : "重命名", GUILayout.Width(60))) editing = !editing;
            }
            GUILayout.EndHorizontal();

            return name;
        }



        //Sub GUIs-------------------------------------------------------
        private void OnGameObjectGUI()
        {
            if (target == null)
            {
                FocusOn(null, SelectionType.Map);
                return;
            }
            DrawPreview();
            GameObject gameObject = target as GameObject;

            gameObject.name = EditableNameField(gameObject.name);

            GUILayout.Label("", Prefs.mapAssetBackgroundStyle, GUILayout.ExpandHeight(true));
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

            prefab.name = EditableNameField(prefab.name);


            if (MapAssetManager.ContainsPrefab(prefab))
            {
                //在表列
                MapAssetManager.CurrentInformation = EditorGUILayout.TextArea(MapAssetManager.CurrentInformation, Prefs.informationStyle);

                GUILayout.Label("", Prefs.mapAssetBackgroundStyle, GUILayout.ExpandHeight(true));
                GUILayout.BeginHorizontal(Prefs.mapAssetBackgroundStyle, GUILayout.Height(28));
                {
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                //不在表列

                GUILayout.Label("", Prefs.mapAssetBackgroundStyle, GUILayout.ExpandHeight(true));
                GUILayout.BeginHorizontal(Prefs.mapAssetBackgroundStyle, GUILayout.Height(28));
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("加入物品表列", GUILayout.Width(120))) { MapAssetManager.AddPrefab(prefab); }
                }
                GUILayout.EndHorizontal();
            }


        }

        private void OnGroupGUI()
        {
            MapGroup group = target as MapGroup;

            if (group == null)
            {
                GUILayout.Label("", Prefs.mapAssetBackgroundStyle, GUILayout.Height(200));
                GUILayout.Label("当前组为空", Prefs.informationStyle);

                GUILayout.Label("", Prefs.mapAssetBackgroundStyle, GUILayout.ExpandHeight(true));
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
                DrawPreview();
                group.groupName = EditableNameField(group.groupName);

                GUILayout.Label("", Prefs.mapAssetBackgroundStyle, GUILayout.ExpandHeight(true));
                GUILayout.BeginHorizontal(Prefs.mapAssetBackgroundStyle, GUILayout.Height(28));
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("保存组", GUILayout.Width(60))) { MapGroupAssetEditor.SaveGroup(group); FocusOn(MapSystem.groupList[MapSystem.currentGroupIndex], SelectionType.Group); }
                    if (GUILayout.Button("卸载组", GUILayout.Width(60))) MapGroupAssetEditor.UnLoadGroup(group);
                    if (MapAssetManager.CurrentAsset != null && GUILayout.Button("加载组", GUILayout.Width(60))) { MapGroupAssetEditor.LoadGroup(MapAssetManager.CurrentAsset, MapSystem.currentGroupIndex); FocusOn(MapSystem.groupList[MapSystem.currentGroupIndex], SelectionType.Group); }
                }
                GUILayout.EndHorizontal();
            }
        }

        private void OnGroupAssetGUI()
        {
            MapGroupAsset groupAsset = target as MapGroupAsset;
            DrawPreview();
            GUILayout.Label(groupAsset.groupName, Prefs.nameStyle);
            groupAsset.information = EditorGUILayout.TextArea(groupAsset.information, Prefs.informationStyle);
            GUILayout.Label("", Prefs.mapAssetBackgroundStyle, GUILayout.ExpandHeight(true));
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
            GUILayout.Label("", Prefs.mapAssetBackgroundStyle, GUILayout.ExpandHeight(true));
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

            ShowDebugMessage();
        }


    }
}
