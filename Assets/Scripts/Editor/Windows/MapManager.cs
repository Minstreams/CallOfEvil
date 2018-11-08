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
    /// 代替Hierarchy的地图编辑器窗口，也是编辑器编辑主要功能
    /// </summary>
    public class MapManager : EditorWindow
    {
        //引用---------------------------------------------------------------------------
        private static MapManagerStyle style;
        public static MapManagerStyle Style { get { return style; } }
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

        /// <summary>
        /// 地图编辑器是否已经激活
        /// </summary>
        public static bool Active { get { return style != null && MapSystem.Active; } }



        //信息
        [System.Serializable]
        private class Pref : SavablePref
        {
            /// <summary>
            /// 标记场上Group是否改变
            /// </summary>
            public List<bool> dirtyMark = new List<bool>();
        }
        private static Pref pref = new Pref();

        [InitializeOnLoadMethod]
        private static void Init()
        {
            pref.Load();
            style = (MapManagerStyle)EditorGUIUtility.Load("Map Manager Style.asset");
            EditorApplication.quitting += pref.Save;
            Debug.Log("Map Manager Inited!");
        }






        //UI基础-----------------------------------------------------------------------
        [MenuItem("自制工具/地图编辑器 _F6")]
        static void OpenWindow()
        {
            EditorWindow.GetWindow<MapInspector>("地图信息", false);
            EditorWindow.GetWindow<MapAssetManager>("地图资源", false);
            EditorWindow.GetWindow<MapManager>("地图编辑器", true);
        }

        private void OnEnable()
        {
            wantsMouseMove = true;
            autoRepaintOnSceneChange = true;
            //targetQuaternion.valueChanged.AddListener(() => Repaint());
            Selection.SetActiveObjectWithContext(null, null);
            Selection.selectionChanged += OnSelectionChanged;
            SceneView.onSceneGUIDelegate += OnSceneFunc;
            MapSystem.InitGroupActiveState();
            Debug.Log("Map Manager Enabled!");
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            SceneView.onSceneGUIDelegate -= OnSceneFunc;
            Debug.Log("Map Manager Disabled!");
        }

        private void OnSceneFunc(SceneView sceneView)
        {
            Repaint();
        }

        private GameObject lastActiveGameObject = null;
        private void OnSelectionChanged()
        {
            if (Active && lastActiveGameObject != null && PrefabUtility.GetPrefabType(lastActiveGameObject) == PrefabType.PrefabInstance)
            {
                Debug.Log("SetMapObject(" + lastActiveGameObject + ");");
                SetMapObject(lastActiveGameObject);
            }
            lastActiveGameObject = Selection.activeGameObject;
        }





        //UI交互------------------------------------------------------------------------
        private AnimQuaternion targetQuaternion = new AnimQuaternion(Quaternion.Euler(0, MapSystem.currentAngle, 0));  //用于记录CurrentAngle
        private AnimFloat viewScale = new AnimFloat(0.066f);   //预览视图缩放量
        private Quaternion cameraRotation;  //场景相机旋转偏移量
        private float radius;       //圆盘半径
        private Vector2 center;     //圆盘中心

        private Quaternion rot = Quaternion.Euler(0, 0, -MapSystem.AnglePerGroup);

        private MouseArea mouseArea;    //记录鼠标所处区域
        private int areaIndex;  //记录选中物体的Index，或其他辅助信息

        private AnimFloat mouseDragAngle = new AnimFloat(0);    //记录拖拽旋转角度

        private Vector2 doubleClickMarkRawPos;
        private AnimFloat doubleClickMarkAlpha = new AnimFloat(0);

        /// <summary>
        /// 鼠标区域位置
        /// </summary>
        private enum MouseArea
        {
            Invalid,
            DragArea,
            GroupArea,
            ObjectArea
        }
        /// <summary>
        /// 判定鼠标所处区域
        /// </summary>
        private void SetMouseArea()
        {
            Vector2 mousePos = Event.current.mousePosition;

            //判定选中物体
            MapGroup group = groupList[MapSystem.currentGroupIndex];
            for (int i = 0; i < group.transform.childCount; i++)
            {
                Vector2 guiPos = GetElementGUIPos(group.transform.GetChild(i).position);

                if (Vector2.Distance(mousePos, guiPos) < Style.elementRadius)
                {
                    areaIndex = i;
                    mouseArea = MouseArea.ObjectArea;
                    return;
                }
            }

            if (Vector2.Distance(mousePos, center) < radius)
            {
                areaIndex = (int)(MapSystem.GetAngle(GetElementWorldPos(Event.current.mousePosition, 0)) / MapSystem.AnglePerGroup);
                mouseArea = MouseArea.GroupArea;
                return;
            }

            mouseArea = MouseArea.DragArea;

        }

        /// <summary>
        /// 画指定地图组上的元素
        /// </summary>
        private void DrawElement(MapGroup group, float alpha)
        {
            for (int i = 0; i < group.transform.childCount; i++)
            {
                Transform child = group.transform.GetChild(i);
                Handles.color = Color.Lerp(Style.backColor, GetElementColor(child.gameObject), alpha);
                Handles.DrawSolidDisc(GetElementGUIPos(child.position), Vector3.back, Style.elementRadius);
                Handles.Label(GetElementGUIPos(child.position), child.name, Style.elementNameStyle);
            }
            Handles.color = Color.white;

        }
        /// <summary>
        /// 获取物体应该显示的颜色
        /// </summary>
        private Color GetElementColor(GameObject go)
        {
            MapUnit unit = go.GetComponent<MapUnit>();
            if (unit == null) return Color.black;
            else return Color.green;
        }
        /// <summary>
        /// 获取物体应该显示的GUI位置
        /// </summary>
        private Vector2 GetElementGUIPos(Vector3 worldPos)
        {
            return center + (Vector2)(cameraRotation * new Vector2(worldPos.x, -worldPos.z) * viewScale.value * radius);
        }
        private Vector3 GetElementWorldPos(Vector2 guiPos, float height)
        {
            Vector3 temp = Quaternion.Inverse(cameraRotation) * (guiPos - center) / viewScale.value / radius;
            return new Vector3(temp.x, height, -temp.y);
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

        private void SetDoubleClickMark()
        {
            doubleClickMarkRawPos = Quaternion.Inverse(cameraRotation) * (Event.current.mousePosition - center);
            doubleClickMarkAlpha.value = 1;
            doubleClickMarkAlpha.target = 0;
        }

        private void OnGUI()
        {
            //激活检测
            if (!Active)
            {
                GUILayout.Label("地图没打开，地图编辑器未激活！");
                return;
            }

            //场景参数计算
            radius = Mathf.Min(EditorGUIUtility.currentViewWidth - Style.sideWidth, position.height) / 2 - Style.edge;
            center = new Vector2(radius + Style.edge + Style.sideWidth, radius + Style.edge);

            Vector3 cameraDir = SceneView.lastActiveSceneView == null ? Vector3.back : SceneView.lastActiveSceneView.camera.transform.forward;
            Vector2 start = Vector3.right;
            cameraRotation = Quaternion.LookRotation(Vector3.forward, new Vector3(cameraDir.x, cameraDir.z, 0));
            Vector2 mid = Quaternion.Euler(0, 0, -MapSystem.AnglePerGroup / 2) * Vector2.right * radius / 2;

            //事件交互
            switch (Event.current.type)
            {
                case EventType.Layout:
                case EventType.Repaint:
                    //画底部圆盘
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = MapSystem.GetAngle(new Vector3(mid.x, 0, -mid.y));
                        Color color = Style.circleGradient.Evaluate(angle / MapSystem.MaxAngle);
                        float alpha = 1 - Style.arcAlphaCurve.Evaluate(Mathf.Abs(MapSystem.SubSigned(MapSystem.currentAngle, angle)));
                        color.a = Mathf.Lerp(0, 1, alpha);
                        Handles.color = color;
                        Handles.DrawSolidArc((Vector3)center + cameraRotation * mid * Mathf.Lerp(Style.arcCenterOffsetRateMin, Style.arcCenterOffsetRateMax, alpha), Vector3.back, cameraRotation * start, 120, radius * Mathf.Lerp(Style.arcMinRadiusRate, 1, alpha));
                        Handles.Label((Vector3)center + cameraRotation * mid * Mathf.Lerp(1, Style.groupNumDistance, alpha), ((int)(angle / MapSystem.AnglePerGroup)).ToString(), Style.groupNumStyle);
                        start = rot * start;
                        mid = rot * mid;
                    }
                    Handles.color = Color.white;

                    //中心圆盘
                    Handles.color = Style.centerDiscColor;
                    {
                        Handles.DrawSolidDisc(center, Vector3.back, Style.centerDiscRadius);
                        Handles.Label(center - Style.centerDiscStyle.CalcSize(new GUIContent(MapSystem.CurrentCircle.ToString())) / 2f, MapSystem.CurrentCircle.ToString(), Style.centerDiscStyle);
                    }
                    Handles.color = Color.white;


                    //画侧面刻度
                    Handles.color = Style.sideColor;
                    {
                        Vector2 yRange = new Vector2(Style.edge + (1 - Style.sideLineLengthRate) * radius, Style.edge + (1 + Style.sideLineLengthRate) * radius);
                        Vector2 xRange = new Vector2(Style.sideWidth - Style.sideXOffset + (Style.sideFlip ? -Style.sideMarkWidth : Style.sideMarkWidth), Style.sideWidth - Style.sideXOffset);
                        Handles.DrawAAPolyLine(
                            Style.sideLineWidth,
                            new Vector2(xRange.x, yRange.x),
                            new Vector2(xRange.y, yRange.x),
                            new Vector2(xRange.y, yRange.y),
                            new Vector2(xRange.x, yRange.y)
                            );

                        xRange.x = Style.sideWidth - Style.sideXOffset + (Style.sideFlip ? -Style.sideMarkWidth * Style.sideLineLengthRate : Style.sideMarkWidth * Style.sideLineLengthRate);
                        for (int i = 1; i < Style.sideMarkNum; i++)
                        {
                            float yPos = Mathf.Lerp(yRange.x, yRange.y, (float)i / (float)Style.sideMarkNum);
                            Handles.DrawLine(new Vector2(xRange.x, yPos), new Vector2(xRange.y, yPos));
                        }

                        float arrowXOffset = (Style.sideFlip ? -1 : 1) * Style.sideArrowSize * Mathf.Cos(Style.sideArrowAngle * Mathf.PI / 180f);
                        float arrowyPos = Mathf.Lerp(yRange.y, yRange.x, (viewScale.value - Style.minScale) / (Style.maxScale - Style.minScale));
                        Handles.ArrowHandleCap(0, new Vector2(xRange.x + arrowXOffset, arrowyPos), Quaternion.Euler(0, (Style.sideFlip ? 90 : -90) + Style.sideArrowAngle, 0), Style.sideArrowSize, EventType.Repaint);
                        Handles.Label(new Vector2(xRange.x + arrowXOffset / 2, arrowyPos), "缩放等级", Style.sideMarkStyle);
                    }
                    Handles.color = Color.white;

                    //画双击指示
                    if (doubleClickMarkAlpha.value > 0)
                    {
                        Handles.color = new Color(1, 1, 1, doubleClickMarkAlpha.value);
                        Handles.DrawSolidDisc(cameraRotation * doubleClickMarkRawPos + (Vector3)center, Vector3.back, (1 - doubleClickMarkAlpha.value) * Style.doubleClickMarkRadius);
                        Handles.color = Color.white;
                    }

                    //画元素
                    Handles.color = Style.elementOutLineColor;
                    {
                        foreach (GameObject g in Selection.gameObjects)
                        {
                            PrefabType prefabType = PrefabUtility.GetPrefabType(g);
                            if (prefabType != PrefabType.Prefab)
                            {
                                Handles.DrawSolidDisc(GetElementGUIPos(g.transform.position), Vector3.back, Style.elementRadius + Style.elementOutlineWidth);
                            }
                        }

                        MapGroup group = groupList[MapSystem.currentGroupIndex];
                        DrawElement(group, 1);

                        group = groupList[MapSystem.GetPrevious(MapSystem.currentGroupIndex)];
                        DrawElement(group, Style.elementAlpha);

                        group = groupList[MapSystem.GetNext(MapSystem.currentGroupIndex)];
                        DrawElement(group, Style.elementAlpha);
                    }
                    Handles.color = Color.white;


                    //控制
                    MapSystem.SetCurrentAngle(QuaternionToAngle(targetQuaternion.value.eulerAngles.y));

                    //视图旋转
                    if (mouseDragAngle.value != 0)
                    {
                        Quaternion deltaRot = Quaternion.Euler(0, mouseDragAngle.value, 0);
                        SceneView.lastActiveSceneView.pivot = deltaRot * SceneView.lastActiveSceneView.pivot;
                        SceneView.lastActiveSceneView.rotation = deltaRot * SceneView.lastActiveSceneView.rotation;
                    }

                    break;
                case EventType.MouseDown:
                    SetMouseArea();
                    switch (mouseArea)
                    {
                        case MouseArea.GroupArea:
                            //双击
                            if (Event.current.clickCount > 1)
                            {
                                SceneView.lastActiveSceneView.LookAt(GetElementWorldPos(Event.current.mousePosition, 0));
                                SetDoubleClickMark();
                            }

                            //选中组
                            Selection.activeGameObject = null;
                            targetQuaternion.target = Quaternion.Euler(0, areaIndex * MapSystem.AnglePerGroup + MapSystem.AnglePerGroup / 2, 0);
                            break;
                        case MouseArea.ObjectArea:
                            //双击
                            if (Event.current.clickCount > 1)
                            {
                                SceneView.lastActiveSceneView.LookAt(Selection.activeGameObject.transform.position);
                                SetDoubleClickMark();
                            }

                            //选中物体
                            Selection.activeGameObject = groupList[MapSystem.currentGroupIndex].transform.GetChild(areaIndex).gameObject;
                            break;
                    }

                    break;
                case EventType.MouseDrag:

                    switch (mouseArea)
                    {
                        case MouseArea.GroupArea:
                        case MouseArea.DragArea:
                            Vector3 mouseDir = Event.current.mousePosition - center;
                            float mouseDirSqrLength = mouseDir.sqrMagnitude;
                            float dragAngle = -Vector3.Cross(mouseDir, Event.current.delta).z / mouseDirSqrLength * 180 / Mathf.PI * Style.dragSensitivity;

                            mouseDragAngle.value = dragAngle;
                            mouseDragAngle.target = 0;

                            break;
                        case MouseArea.ObjectArea:
                            Selection.activeGameObject.transform.position = GetElementWorldPos(Event.current.mousePosition, Selection.activeGameObject.transform.position.y);
                            break;
                    }

                    break;

                case EventType.ScrollWheel:
                    viewScale.target = Mathf.Clamp(viewScale.target - Event.current.delta.y * Style.scaleSensitivity, Style.minScale, Style.maxScale);
                    break;
            }



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
