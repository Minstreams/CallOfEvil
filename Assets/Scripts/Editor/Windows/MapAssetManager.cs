using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace EditorSystem
{
    /// <summary>
    /// 代替Assets的资源窗口
    /// </summary>
    public class MapAssetManager : EditorWindow
    {
        //引用
        private static MapAssetManager instance = null;
        private static EditorMatrixPrefs Prefs { get { return EditorMatrix.Prefs; } }
        private static PrefabDictionary PrefabInformation { get { return Prefs.prefabDictionary; } }
        private static List<MapGroupAsset> MapGroupAssets { get { return Prefs.mapGroupAssets; } }


        //通用辅助配置字段
        private int toolbarSelected = 0;
        private readonly string[] toolbarTitle = { "物体", "地图组预设" };
        private Vector2 scollVector;
        //物体
        private static int prefabSelected = 0;
        public static GameObject CurrentPrefab { get { return PrefabInformation.Count == 0 ? null : PrefabInformation.Keys[prefabSelected]; } }
        public static string CurrentInformation { get { return PrefabInformation.Count == 0 ? "" : PrefabInformation[CurrentPrefab]; } set { PrefabInformation[CurrentPrefab] = value; } }
        public static bool ContainsPrefab(GameObject prefab) { return PrefabInformation.ContainsKey(prefab); }
        public static void AddPrefab(GameObject prefab) { PrefabInformation.Add(prefab, "物品说明"); prefabSelected = PrefabInformation.Count - 1; if (instance != null) instance.Repaint(); }
        public static void DeletePrefab(GameObject prefab) { PrefabInformation.Remove(prefab); if (instance != null) instance.Repaint(); }

        private GameObject tempPrefabInstance = null;
        private bool validPos;//所在位置是否合法？当鼠标悬停在场景或编辑器窗口时位置是合法的

        //地图组
        private static int mapAssetSelected = 0;
        public static MapGroupAsset CurrentAsset { get { return MapGroupAssets.Count == 0 ? null : MapGroupAssets[mapAssetSelected]; } }





        private void OnEnable()
        {
            instance = this;
        }
        private void OnGUI()
        {
            toolbarSelected = GUILayout.Toolbar(toolbarSelected, toolbarTitle, Prefs.toolbarStyle);

            if (toolbarSelected == 0)
            {
                //物体

                if (PrefabInformation.Count == 0)
                {
                    //大小为空
                    GUILayout.Label("……", Prefs.mapAssetBackgroundStyle, GUILayout.ExpandHeight(true));
                }
                else
                {
                    //selection list
                    scollVector = GUILayout.BeginScrollView(scollVector);
                    {
                        List<string> assetNameList = new List<string>();
                        for (int i = 0; i < PrefabInformation.Count; i++)
                        {
                            assetNameList.Add(PrefabInformation.Keys[i].name);
                        }
                        EditorGUI.BeginChangeCheck();
                        prefabSelected = GUILayout.SelectionGrid(prefabSelected, assetNameList.ToArray(), 1, Prefs.mapAssetBackgroundStyle, GUILayout.Height(27 * PrefabInformation.Count));
                        if (EditorGUI.EndChangeCheck()) MapInspector.FocusOn(CurrentPrefab, MapInspector.SelectionType.Prefab);

                        GUILayout.Label("", Prefs.mapAssetBackgroundStyle, GUILayout.ExpandHeight(true));
                    }
                    GUILayout.EndScrollView();

                    //function
                    GameObject cp = CurrentPrefab;
                    GUILayout.BeginHorizontal(Prefs.mapAssetBackgroundStyle, GUILayout.Height(28));
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("删除", GUILayout.Width(60))) { DeletePrefab(cp); prefabSelected--; MapInspector.FocusOn(CurrentPrefab, MapInspector.SelectionType.Prefab); }
                    }
                    GUILayout.EndHorizontal();


                    //drag the prefab out
                    switch (Event.current.type)
                    {
                        case EventType.MouseDown:
                            tempPrefabInstance = PrefabUtility.InstantiatePrefab(CurrentPrefab) as GameObject;
                            Debug.Log(tempPrefabInstance);
                            break;
                        case EventType.MouseDrag:
                            if (tempPrefabInstance == null) break;
                            Vector3 prefabPos = Vector3.zero;
                            if (mouseOverWindow == null)
                            {
                                validPos = false;
                            }
                            else if (mouseOverWindow.GetType() == typeof(MapManager))
                            {
                                MapManager mapManager = mouseOverWindow as MapManager;
                                Vector2 guiPos = Event.current.mousePosition + position.position - mapManager.position.position;
                                prefabPos = mapManager.GetElementWorldPos(guiPos, 0);
                                validPos = true;
                            }
                            else if (mouseOverWindow.GetType() == typeof(SceneView))
                            {
                                SceneView sv = SceneView.lastActiveSceneView;
                                Handles.SetCamera(SceneView.lastActiveSceneView.camera);
                                Vector2 guiPos = Event.current.mousePosition + new Vector2(position.xMin - sv.position.xMin, position.yMax - sv.position.yMax);
                                Ray ray = HandleUtility.GUIPointToWorldRay(guiPos);
                                float yRate = ray.origin.y / ray.direction.y;
                                prefabPos = new Vector3(ray.origin.x - ray.direction.x * yRate, 0, ray.origin.z - ray.direction.z * yRate);
                                validPos = true;
                            }
                            else
                            {
                                validPos = false;
                            }
                            tempPrefabInstance.transform.position = prefabPos;
                            break;
                        case EventType.MouseUp:
                            DestroyImmediate(tempPrefabInstance);
                            break;
                        case EventType.Ignore:
                            if (validPos)
                            {
                                MapManager.SetMapObject(tempPrefabInstance);
                            }
                            else
                            {
                                DestroyImmediate(tempPrefabInstance);
                            }
                            break;
                    }
                }




            }
            else
            {
                //地图组预设
                if (MapGroupAssets.Count == 0)
                {
                    //大小为空
                    GUILayout.Label("……", Prefs.mapAssetBackgroundStyle, GUILayout.ExpandHeight(true));
                }
                else
                {
                    //selection list
                    scollVector = GUILayout.BeginScrollView(scollVector);
                    {
                        List<string> assetNameList = new List<string>();
                        for (int i = 0; i < MapGroupAssets.Count; i++)
                        {
                            assetNameList.Add(MapGroupAssets[i].groupName + (MapGroupAssets[i].baked ? "" : Prefs.mapAssetUnbakedStateMark));
                        }
                        EditorGUI.BeginChangeCheck();
                        mapAssetSelected = GUILayout.SelectionGrid(mapAssetSelected, assetNameList.ToArray(), 1, Prefs.mapAssetBackgroundStyle, GUILayout.Height(27 * MapGroupAssets.Count));
                        if (EditorGUI.EndChangeCheck()) MapInspector.FocusOn(CurrentAsset, MapInspector.SelectionType.GroupAsset);

                        GUILayout.Label("", Prefs.mapAssetBackgroundStyle, GUILayout.ExpandHeight(true));
                    }
                    GUILayout.EndScrollView();

                    //function
                    MapGroupAsset mga = CurrentAsset;
                    GUILayout.BeginHorizontal(Prefs.mapAssetBackgroundStyle, GUILayout.Height(28));
                    {
                        GUILayout.FlexibleSpace();
                        if (!mga.baked && GUILayout.Button("烘焙", GUILayout.Width(60))) MapGroupAssetEditor.BakeGroupAsset(mga);
                        if (GUILayout.Button("删除", GUILayout.Width(60))) { MapGroupAssetEditor.DeleteGroupAsset(mga); mapAssetSelected = 0; MapInspector.FocusOn(CurrentAsset, MapInspector.SelectionType.GroupAsset); }
                    }
                    GUILayout.EndHorizontal();
                }
            }



        }
    }
}
