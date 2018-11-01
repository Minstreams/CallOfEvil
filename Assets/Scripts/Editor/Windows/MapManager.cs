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
        /// <summary>
        /// 地图组预设
        /// </summary>
        public static List<MapGroupAsset> MapGroupAssets { get { return Style.mapGroupAssets; } }
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
            EditorWindow.GetWindow<MapInspector>("地图信息", false);
            EditorWindow.GetWindow<MapManager>("地图编辑器", true);
        }

        private void OnEnable()
        {
            wantsMouseMove = true;
            autoRepaintOnSceneChange = true;
            targetQuaternion.valueChanged.AddListener(() => Repaint());
            Selection.selectionChanged += OnSelectionChanged;
            SceneView.onSceneGUIDelegate += OnSceneFunc;
            EditorApplication.hierarchyChanged += OnSelectionChanged;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            SceneView.onSceneGUIDelegate -= OnSceneFunc;
            EditorApplication.hierarchyChanged -= OnSelectionChanged;
        }

        private void OnSceneFunc(SceneView sceneView)
        {
            Repaint();
        }

        private GameObject lastActive = null;
        private void OnSelectionChanged()
        {
            if (MapSystem.Active && lastActive != null && PrefabUtility.GetPrefabType(lastActive) == PrefabType.PrefabInstance)
            {
                SetMapObject(lastActive);
                if (lastActive == Selection.activeGameObject)
                {
                    lastActive = null;
                    //Selection.activeGameObject = null;
                    return;
                }
            }
            lastActive = Selection.activeGameObject;
        }

        private AnimQuaternion targetQuaternion = new AnimQuaternion(Quaternion.identity);
        private float viewScale = 20;
        private Quaternion cameraRotation;
        float edge = 20;
        float radius;
        Vector2 center;

        Quaternion rot = Quaternion.Euler(0, 0, -MapSystem.AnglePerGroup);

        private void OnGUI()
        {
            radius = EditorGUIUtility.currentViewWidth / 2 - 20;
            center = Vector2.one * (radius + edge);

            Vector3 cameraDir = SceneView.lastActiveSceneView == null ? Vector3.back : SceneView.lastActiveSceneView.camera.transform.forward;
            Vector2 start = Vector3.right;
            cameraRotation = Quaternion.LookRotation(Vector3.forward, new Vector3(cameraDir.x, cameraDir.z, 0));
            Vector2 mid = Quaternion.Euler(0, 0, -MapSystem.AnglePerGroup / 2) * Vector2.right * radius / 2;


            switch (Event.current.type)
            {
                case EventType.Repaint:
                    //画底部圆盘

                    for (int i = 0; i < 3; i++)
                    {
                        float angle = MapSystem.GetAngle(new Vector3(mid.x, 0, -mid.y));
                        Color color = Style.circleGradient.Evaluate(angle / MapSystem.MaxAngle);
                        float alpha = 1 - Style.arcAlphaCurve.Evaluate(Mathf.Abs(MapSystem.SubSigned(MapSystem.currentAngle, angle)));
                        color.a = Mathf.Lerp(0, 1, alpha);
                        Handles.color = color;
                        Handles.DrawSolidArc(center, Vector3.back, cameraRotation * start, 120, radius);
                        Handles.Label((Vector3)center + cameraRotation * mid, ((int)(angle / MapSystem.AnglePerGroup)).ToString());
                        start = rot * start;
                        mid = rot * mid;
                    }

                    Handles.color = Color.white;


                    //画元素
                    MapGroup group = groupList[MapSystem.currentGroupIndex];
                    DrawElement(group, 1);

                    group = groupList[MapSystem.GetPrevious(MapSystem.currentGroupIndex)];
                    DrawElement(group, Style.elementAlpha);

                    group = groupList[MapSystem.GetNext(MapSystem.currentGroupIndex)];
                    DrawElement(group, Style.elementAlpha);

                    break;
                case EventType.MouseDown:
                    Vector2 mouseMinusCenter = Quaternion.Inverse(cameraRotation) * (Event.current.mousePosition - center);
                    Vector3 mouseMinusCenterOnGround = new Vector3(mouseMinusCenter.x, 0, -mouseMinusCenter.y);
                    float targetAngle = ((int)(MapSystem.GetAngle(mouseMinusCenterOnGround) / MapSystem.AnglePerGroup)) * MapSystem.AnglePerGroup + MapSystem.AnglePerGroup / 2;

                    targetQuaternion.target = Quaternion.Euler(0, targetAngle, 0);



                    break;
            }
            //控制
            viewScale = EditorGUILayout.Slider("Scale", viewScale, Style.minScale, Style.maxScale);
            MapSystem.SetCurrentAngle(QuaternionToAngle(targetQuaternion.value.eulerAngles.y));

        }

        private void DrawElement(MapGroup group, float alpha)
        {
            for (int i = 0; i < group.transform.childCount; i++)
            {
                Transform child = group.transform.GetChild(i);
                Vector3 worldPos = child.position;
                Vector2 guiDir = new Vector2(worldPos.x, -worldPos.z) * viewScale * radius;
                Handles.color = Color.Lerp(Style.backColor, GetColor(child.gameObject), alpha);
                Handles.DrawSolidDisc((Vector3)center + cameraRotation * guiDir, Vector3.back, Style.elementRadius);
            }
            Handles.color = Color.white;

        }

        private float QuaternionToAngle(float y)
        {
            //四元数和角度的转换
            int index = MapSystem.currentGroupIndex % 3;
            if (y > 240 && index == 0)
            {
                return MapSystem.Rotate(MapSystem.CurrentCircle * 360, y - 360);
            }
            else if (y < 120 && index == 2)
            {
                return MapSystem.Rotate(MapSystem.CurrentCircle * 360, y + 360);
            }
            return MapSystem.CurrentCircle * 360 + y;
        }

        private Color GetColor(GameObject go)
        {
            MapUnit unit = go.GetComponent<MapUnit>();
            if (unit == null) return Color.black;
            else return Color.green;
        }






        //地图生成控制的编辑器方法-------------------------------------------------------

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
                    pref.dirtyMark[group.index] = false;
                }
            }
            else
            {
                MapGroupAssets.Add(MapGroupAssetEditor.CreateGroupAsset(group));
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
            g.tag = "MapSystem";

            group.index = index;

            groupList[index] = group;
        }





        //场景编辑方法-------------------------------------------------------------------
        /// <summary>
        /// 将GameObject设为对应Group的子物体
        /// </summary>
        public static void SetMapObject(GameObject g)
        {
            if (!g.CompareTag("Untagged")) return;
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
