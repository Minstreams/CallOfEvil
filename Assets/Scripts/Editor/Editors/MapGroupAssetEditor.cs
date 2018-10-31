using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using GameSystem;


/// <summary>
/// 实现GroupAsset的创建删除功能
/// </summary>
[CustomEditor(typeof(MapGroupAsset))]
public class MapGroupAssetEditor : Editor
{
    /// <summary>
    /// 存放场景组数据的根目录
    /// </summary>
    public const string AssetPath = "Assets/Scenes/GroupAssets/";


    //Asset的创建/删除功能

    //保存到Asset中
    public static void SaveToAsset(MapGroup group, MapGroupAsset asset)
    {
        PrefabUtility.ReplacePrefab(group.gameObject, asset.groupPrefab, ReplacePrefabOptions.ConnectToPrefab);
    }


    /// <summary>
    /// 创建Asset，请调用前确保没有重名Asset
    /// </summary>
    public static MapGroupAsset CreateGroupAsset(MapGroup group)
    {
        string groupName = group.groupName;
        string path = AssetPath + groupName + "/" + groupName;
        int index = group.index;

        //创建Asset
        MapGroupAsset asset = ScriptableObject.CreateInstance<MapGroupAsset>();
        asset.groupName = groupName;

        //初始化
        group.transform.position = Vector3.zero;
        group.index = 0;
        group.transform.rotation = Quaternion.identity;

        //绑定Prefab
        asset.groupPrefab = PrefabUtility.CreatePrefab(path + ".prefab", group.gameObject, ReplacePrefabOptions.ConnectToPrefab);

        //创建场景
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        EditorSceneManager.MoveGameObjectToScene(group.gameObject, scene);

        //保存场景
        //这里path的命名不是很懂，scene居然不能直接改名？
        {
            EditorSceneManager.SaveScene(scene, path + "0.unity");

            group.transform.rotation = Quaternion.Euler(0, MapSystem.AnglePerGroup, 0);
            PrefabUtility.RecordPrefabInstancePropertyModifications(group.transform);
            EditorSceneManager.SaveScene(scene, path + "1.unity");

            group.transform.rotation = Quaternion.Euler(0, MapSystem.AnglePerGroup * 2, 0);
            PrefabUtility.RecordPrefabInstancePropertyModifications(group.transform);
            EditorSceneManager.SaveScene(scene, path + "2.unity");
        }

        //重新载入
        EditorSceneManager.CloseScene(scene, true);
        Scene newScene = EditorSceneManager.OpenScene(path + index % 3 + ".unity", OpenSceneMode.Additive);
        MapGroup newGroup = newScene.GetRootGameObjects()[0].GetComponent<MapGroup>();
        newGroup.index = index;
        MapSystem.groupList[index] = newGroup;

        //保存
        AssetDatabase.CreateAsset(asset, path + ".asset");

        return asset;
    }

    public static void BakeGroupAsset(MapGroupAsset groupAsset)
    {
        //TODO:设置实时光照配置
        //Lightmapping.realtimeGI = true;
        //Bake
        throw new System.NotImplementedException();
    }

    public static void DeleteGroupAsset(MapGroupAsset groupAsset)
    {
        //先卸载所有已经存在场上的Group？
        //可能会有多余场景残留？
        //姑且先解绑prefab
        foreach (MapGroup group in MapSystem.groupList)
        {
            if (group.groupName == groupAsset.name)
                PrefabUtility.DisconnectPrefabInstance(group.gameObject);
        }

        //取消注册
        MapSystem.MapGroupAssets.Remove(groupAsset.groupName);

        //删除文件夹
        AssetDatabase.DeleteAsset(AssetPath + groupAsset.groupName);
    }

}
