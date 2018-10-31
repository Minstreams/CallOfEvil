using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.AnimatedValues;
using GameSystem;


namespace EditorSystem
{
    /// <summary>
    /// 地图编辑器窗口
    /// </summary>
    public class MapManager : EditorWindow
    {
        //引用---------------------------------------------------------------------------
        private static MapManagerStyle style;
        public static MapManagerStyle Style { get { if (style == null) { style = (MapManagerStyle)EditorGUIUtility.Load("Map Manager Style.asset"); } return style; } }
        /// <summary>
        /// 组记录表
        /// </summary>
        public static List<MapGroup> groupList { get { return MapSystem.groupList; } }





        //信息
        [System.Serializable]
        private class Pref : SavablePref
        {
            public List<bool> dirtyMark = new List<bool>();
        }

        private static Pref pref = new Pref();

        [InitializeOnLoadMethod]
        private static void Init()
        {
            pref.Load();
            EditorApplication.quitting += pref.Save;
        }






        //编辑器UI-----------------------------------------------------------------------
        [MenuItem("自制工具/地图编辑器 _F6")]
        static void OpenWindow()
        {
            EditorWindow.GetWindow<MapManager>("地图编辑器");
        }

        private void OnGUI()
        {
        }







        //地图生成控制的编辑器方法-------------------------------------------------------

        public static void SaveGroup(MapGroup group)
        {
            if (MapSystem.MapGroupAssets.ContainsKey(group.groupName))
            {
                if (EditorUtility.DisplayDialog("温馨小提示", "场景组已存在，是否覆盖？", "覆盖", "取消"))
                {
                    MapGroupAssetEditor.SaveToAsset(group, MapSystem.MapGroupAssets[group.groupName]);
                    pref.dirtyMark[group.index] = false;
                }
            }
            else
            {
                MapSystem.MapGroupAssets.Add(group.groupName, MapGroupAssetEditor.CreateGroupAsset(group));
            }
        }

        /// <summary>
        /// 尝试卸载Group，并决定卸载前是否，若取消卸载返回false
        /// </summary>
        public static bool UnLoadGroup(MapGroup group)
        {
            if (pref.dirtyMark[group.index])
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

        public static void LoadGroup(MapGroupAsset asset, int index)
        {
            if (groupList[index] != null && !UnLoadGroup(groupList[index])) return;
            MapSystem.LoadGroup(asset, index);
        }

        public static void NewEmptyGroup(int index)
        {
            if (groupList[index] != null && !UnLoadGroup(groupList[index])) return;

            GameObject g = new GameObject(MapGroup.defaultName);
            g.transform.SetParent(MapSystem.mapSystemComponent.transform, true);
            g.transform.position = Vector3.zero;
            g.transform.rotation = Quaternion.Euler(0, MapSystem.AnglePerGroup * (index % 3), 0);

            MapGroup group = g.AddComponent<MapGroup>();

            group.index = index;

            groupList[index] = group;
        }




        //场景编辑方法-------------------------------------------------------------------
    }
}
