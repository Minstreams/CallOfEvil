using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using GameSystem;

namespace EditorSystem
{
    /// <summary>
    /// 实现GroupAsset的创建删除功能
    /// </summary>
    [CustomEditor(typeof(MapGroupAsset))]
    public class MapGroupAssetEditor : Editor
    {
        /// <summary>
        /// MapSystem的组记录表
        /// </summary>
        private static List<MapGroup> groupList { get { return MapSystem.groupList; } }
        /// <summary>
        /// 存放场景组数据的根目录
        /// </summary>
        public const string AssetPath = "Assets/Scenes/GroupAssets/";


        //Asset的创建/删除功能
        //底层维护方法

        public static string GetScenePath(MapGroupAsset asset, int index)
        {
            return AssetPath + asset.groupName + "/" + asset.groupName + index % 3 + ".unity";
        }
        private static void LoadGroupDirect(MapGroupAsset asset, int index)
        {
            Scene newScene = EditorSceneManager.OpenScene(GetScenePath(asset, index), OpenSceneMode.Additive);
            MapGroup newGroup = newScene.GetRootGameObjects()[0].GetComponent<MapGroup>();
            newGroup.index = index;
            MapSystem.groupList[index] = newGroup;
        }

        /// <summary>
        /// 保存到Asset中
        /// </summary>
        public static void SaveToAsset(MapGroup group, MapGroupAsset asset)
        {
            int index = group.index;
            group.index = -1;
            Quaternion rot = group.transform.rotation;
            group.transform.rotation = Quaternion.identity;
            PrefabUtility.ReplacePrefab(group.gameObject, asset.groupPrefab, ReplacePrefabOptions.ConnectToPrefab);
            asset.baked = false;
            group.transform.rotation = rot;
            group.index = index;
        }
        /// <summary>
        /// 创建Asset，请调用前确保没有重名Asset
        /// </summary>
        public static void CreateGroupAsset(MapGroup group)
        {
            string groupName = group.groupName;
            string path = AssetPath + groupName + "/" + groupName;
            int index = group.index;

            //创建Asset
            MapGroupAsset asset = ScriptableObject.CreateInstance<MapGroupAsset>();
            asset.groupName = groupName;
            AssetDatabase.CreateFolder("Assets/Scenes/GroupAssets", groupName);

            //初始化
            group.transform.position = Vector3.zero;
            group.index = -1;
            group.transform.rotation = Quaternion.identity;
            bool isRootOfScene = group.transform.parent == null;
            group.transform.SetParent(null);

            //绑定Prefab
            asset.groupPrefab = PrefabUtility.CreatePrefab(path + ".prefab", group.gameObject, ReplacePrefabOptions.ConnectToPrefab);

            //创建场景
            Scene activeScene = EditorSceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            EditorSceneManager.SetActiveScene(activeScene);

            Scene oldScene = group.gameObject.scene;
            EditorSceneManager.MoveGameObjectToScene(group.gameObject, scene);
            if (isRootOfScene)
                EditorSceneManager.CloseScene(oldScene, true);

            //保存场景
            //这里path的命名不是很懂，scene居然不能直接改名？
            {
                EditorSceneManager.SaveScene(scene, path + "0.unity");

                group.transform.rotation = Quaternion.Euler(0, -MapSystem.AnglePerGroup, 0);
                PrefabUtility.RecordPrefabInstancePropertyModifications(group.transform);
                EditorSceneManager.SaveScene(scene, path + "1.unity");

                group.transform.rotation = Quaternion.Euler(0, -MapSystem.AnglePerGroup * 2, 0);
                PrefabUtility.RecordPrefabInstancePropertyModifications(group.transform);
                EditorSceneManager.SaveScene(scene, path + "2.unity");
            }

            //重新载入
            EditorSceneManager.CloseScene(scene, true);
            LoadGroupDirect(asset, index);

            //保存
            AssetDatabase.CreateAsset(asset, AssetPath + groupName + ".asset");

            MapGroupAssets.Add(asset);
            EditorUtility.SetDirty(EditorMatrix.Prefs);
        }
        /// <summary>
        /// 生成烘焙数据
        /// </summary>
        public static void BakeGroupAsset(MapGroupAsset groupAsset)
        {
            //TODO:设置实时光照配置
            //保护现场
            SceneSetup[] sceneSetup = EditorSceneManager.GetSceneManagerSetup();

            //Setting
            for (int i = 0; i < 3; i++)
            {
                Scene s = EditorSceneManager.OpenScene(GetScenePath(groupAsset, i), OpenSceneMode.Single);
                {
                    Lightmapping.bakedGI = false;
                    Lightmapping.realtimeGI = true;
                    LightmapEditorSettings.realtimeResolution = 4;
                }
                EditorSceneManager.SaveScene(s);
            }

            //恢复现场
            EditorSceneManager.RestoreSceneManagerSetup(sceneSetup);

            //Bake
            string[] paths = { GetScenePath(groupAsset, 0), GetScenePath(groupAsset, 1), GetScenePath(groupAsset, 2) };
            Lightmapping.BakeMultipleScenes(paths); //这个API是异步的！

            groupAsset.baked = true;
        }
        /// <summary>
        /// 删除Asset
        /// </summary>
        /// <param name="groupAsset"></param>
        public static void DeleteGroupAsset(MapGroupAsset groupAsset)
        {
            //先卸载所有已经存在场上的Group？
            //可能会有多余场景残留？
            //姑且先解绑prefab
            foreach (MapGroup group in MapSystem.groupList)
            {
                if (group != null && group.groupName == groupAsset.groupName)
                {
                    PrefabUtility.DisconnectPrefabInstance(group.gameObject);

                    group.dirty = true;
                    //转移，删除场景
                    if (group.transform.parent == null)
                    {
                        Scene toDelete = group.gameObject.scene;
                        group.transform.SetParent(MapSystem.mapSystemComponent.transform);
                        EditorSceneManager.CloseScene(toDelete, true);
                    }
                }
            }

            //取消注册
            MapGroupAssets.Remove(groupAsset);

            //删除文件夹
            AssetDatabase.DeleteAsset(AssetPath + groupAsset.groupName + ".asset");
            //AssetDatabase.DeleteAsset(AssetPath + groupAsset.groupName);

            AssetDatabase.MoveAssetToTrash(AssetPath + groupAsset.groupName);

            EditorUtility.SetDirty(EditorMatrix.Prefs);
            AssetDatabase.SaveAssets();
        }
        /// <summary>
        /// 重命名
        /// </summary>
        public static void RenameGroupAsset(MapGroupAsset groupAsset)
        {
            throw new System.NotImplementedException();
        }


        //地图生成控制的编辑器方法-------------------------------------------------------
        /// <summary>
        /// 地图组预设
        /// </summary>
        public static List<MapGroupAsset> MapGroupAssets { get { return EditorMatrix.Prefs.mapGroupAssets; } }
        public static bool ContainsAsset(string name)
        {
            foreach (MapGroupAsset asset in MapGroupAssets)
            {
                if (asset.groupName == name) return true;
            }
            return false;
        }
        public static MapGroupAsset GetGroupAssetByName(string name)
        {
            foreach (MapGroupAsset asset in MapGroupAssets)
            {
                if (asset.groupName == name) return asset;
            }
            return null;
        }
        /// <summary>
        /// 存储Group
        /// </summary>
        public static void SaveGroup(MapGroup group)
        {
            if (ContainsAsset(group.groupName))
            {
                if (EditorUtility.DisplayDialog("温馨小提示", "场景组已存在，是否覆盖？", "覆盖", "取消"))
                {
                    MapGroupAssetEditor.SaveToAsset(group, GetGroupAssetByName(group.groupName));
                    group.dirty = false;
                }
            }
            else
            {
                CreateGroupAsset(group);
            }
        }
        /// <summary>
        /// 尝试卸载Group，并决定卸载前是否，若取消卸载返回false
        /// </summary>
        public static bool UnLoadGroup(MapGroup group)
        {
            if (group == null) return true;
            if (group.dirty)
                switch (EditorUtility.DisplayDialogComplex("温馨小提示", "当前场景组未保存，是否保存该组？", "保存", "取消", "不保存"))
                {
                    case 0:
                        SaveGroup(group);
                        MapSystem.UnLoadGroup(group);
                        return true;
                    case 2:
                        MapSystem.UnLoadGroup(group);
                        return true;
                    default:
                        return false;
                }
            else
            {
                MapSystem.UnLoadGroup(group);
                return true;
            }
        }
        /// <summary>
        /// 加载一个Group到指定位置
        /// </summary>
        public static void LoadGroup(MapGroupAsset asset, int index)
        {
            if (groupList[index] != null && !UnLoadGroup(groupList[index])) return;
            LoadGroupDirect(asset, index);
        }
        /// <summary>
        /// 在指定位置创建空的Group
        /// </summary>
        public static void NewEmptyGroup(int index)
        {
            if (groupList[index] != null && !UnLoadGroup(groupList[index])) return;

            GameObject g = new GameObject(MapGroup.defaultName, typeof(MapGroup));
            g.transform.SetParent(MapSystem.mapSystemComponent.transform, true);
            g.transform.position = Vector3.zero;
            g.transform.rotation = Quaternion.Euler(0, -MapSystem.AnglePerGroup * (index % 3), 0);

            MapGroup group = g.GetComponent<MapGroup>();
            g.tag = "MapSystem";

            group.index = index;
            group.dirty = true;

            groupList[index] = group;
        }
    }
}