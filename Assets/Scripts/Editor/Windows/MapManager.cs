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
            MapManager window = EditorWindow.GetWindow<MapManager>("地图编辑器");
            window.autoRepaintOnSceneChange = true;
        }

        private void OnGUI()
        {
            Handles.DrawSolidArc(Event.current.mousePosition, Vector3.forward, Vector3.down, 60, 250);
            Handles.Label(Event.current.mousePosition, "TestHandle");


            GUILayout.Label("Test");
        }







        //地图生成控制的编辑器方法-------------------------------------------------------

        /// <summary>
        /// 存储Group
        /// </summary>
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
        /// <summary>
        /// 加载一个Group到指定位置
        /// </summary>
        public static void LoadGroup(MapGroupAsset asset, int index)
        {
            if (groupList[index] != null && !UnLoadGroup(groupList[index])) return;
            MapSystem.LoadGroup(asset, index);
        }
        /// <summary>
        /// 在指定位置创建空的Group
        /// </summary>
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
        /// <summary>
        /// 将GameObject设为对应Group的子物体
        /// </summary>
        public static void SetMapObject(GameObject g)
        {
            MapGroup group = g.GetComponentInParent<MapGroup>();
            float gAngle = MapSystem.GetAngle(g.transform.position);
            int gIndex = (int)(gAngle / MapSystem.AnglePerGroup);
            MapUnit[] units = g.GetComponentsInChildren<MapUnit>();

            if (group == null)
            {
                //新添加
                group = groupList[gIndex];

                g.transform.SetParent(group.transform, true);

                foreach (MapUnit unit in units)
                {
                    AddUnit(unit, group);
                }
            }
            else if (gIndex != group.index)
            {
                //换组
                MapGroup newGroup = groupList[gIndex];
                g.transform.SetParent(newGroup.transform, true);

                foreach (MapUnit unit in units)
                {
                    DeleteUnit(unit);
                    AddUnit(unit, newGroup);
                }
            }
            else
            {
                //调整
                foreach (MapUnit unit in units)
                {
                    AdjustUnit(unit);
                }
            }
        }

        public static void AddUnit(MapUnit unit, MapGroup group)
        {
            unit.angle = MapSystem.GetAngle(unit.transform.position);
            unit.group = group;
            List<MapUnit> list = group.unitList;

            int i = 0;
            while (i < list.Count && list[i].angle < unit.angle) i++;

            list.Insert(i, unit);
            unit.index = i;

            for (int j = i + 1; j < list.Count; j++)
            {
                list[j].index = j;
            }
        }

        public static void DeleteUnit(MapUnit unit)
        {
            MapGroup group = unit.group;

            group.unitList.RemoveAt(unit.index);
            unit.group = null;

            for (int j = unit.index; j < group.unitList.Count; j++)
            {
                group.unitList[j].index = j;
            }
        }

        public static void AdjustUnit(MapUnit unit)
        {
            float angle = MapSystem.GetAngle(unit.transform.position);
            MapGroup group = unit.group;
            List<MapUnit> list = group.unitList;

            if (MapSystem.SubSigned(angle, unit.angle) > 0)
                for (int i = unit.index + 1; i < list.Count && list[i].angle < angle; i++)
                {
                    list[i - 1] = list[i];
                    list[i] = unit;
                }
            else
                for (int i = unit.index - 1; i >= 0 && list[i].angle > angle; i--)
                {
                    list[i + 1] = list[i];
                    list[i] = unit;
                }
        }
    }
}
