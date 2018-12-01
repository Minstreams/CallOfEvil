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
        /// <summary>
        /// 各种参数
        /// </summary>
        private static EditorMatrixPrefs Prefs { get { return EditorMatrix.Prefs; } }
        /// <summary>
        /// MapSystem的组记录表
        /// </summary>
        private static List<MapGroup> GroupList { get { return MapSystem.groupList; } }
        /// <summary>
        /// 地图编辑器是否已经激活
        /// </summary>
        public static bool Active { get { return isWindowOpen && MapSystem.Active && !EditorApplication.isPlaying; } }



        //窗口基础-----------------------------------------------------------------------
        [MenuItem("自制工具/地图编辑器 _F6")]
        static void OpenWindow()
        {
            EditorWindow.GetWindow<MapInspector>("地图信息", false);
            EditorWindow.GetWindow<MapAssetManager>("地图资源", false);
            EditorWindow.GetWindow<MapManager>("地图编辑器", true);
        }
        private static bool isWindowOpen;
        private void RepaintOnSceneGUI(SceneView sceneView) { Repaint(); }
        private void OnEnable()
        {
            isWindowOpen = true;
            autoRepaintOnSceneChange = true;
            Selection.SetActiveObjectWithContext(null, null);
            SceneView.onSceneGUIDelegate += RepaintOnSceneGUI;    //随AnimatedMaterials刷新而刷新，几乎是一直刷新了
            Selection.selectionChanged += OnSelectionChanged;
            invalidObjectList = new List<GameObject>();
            Debug.Log("Map Manager Enabled!");
            if (!Active) return;
            TemporaryTool.RearrangeInvalidMapGroup();
        }
        private void OnDisable()
        {
            isWindowOpen = false;
            SceneView.onSceneGUIDelegate -= RepaintOnSceneGUI;
            Selection.selectionChanged -= OnSelectionChanged;
            Debug.Log("Map Manager Disabled!");
        }





        //选择检查/修正------------------------------------------------------------------
        private bool selectionChecked = false;
        private int counter;
        private void OnSelectionChanged()
        {
            selectionChecked = false;
        }
        private void CheckSelection()
        {
            if (!Active) return;
            selectionChecked = true;
            //Debug.Log("Selection changed.");

            //检查选择
            if (Selection.activeGameObject == null) return;
            if (Selection.activeGameObject.tag == "MapSystem")
            {
                if (Selection.activeGameObject.transform.childCount == 0)
                {
                    Selection.activeGameObject = null;
                    return;
                }
                else
                {
                    while (counter >= Selection.activeGameObject.transform.childCount) counter -= Selection.activeGameObject.transform.childCount;
                    Selection.activeGameObject = Selection.activeGameObject.transform.GetChild(counter).gameObject;
                    counter++;
                    selectionChecked = false;
                    return;
                }

            }
            if (PrefabUtility.GetPrefabType(Selection.activeGameObject) == PrefabType.Prefab)
            {
                MapInspector.FocusOn(Selection.activeGameObject, MapInspector.SelectionType.Prefab);
                return;
            }

            //修正选择
            Transform parent = Selection.activeGameObject.transform.parent;
            if (parent != null)
            {
                if (parent.tag != "MapSystem")
                {
                    Selection.activeGameObject = parent.gameObject;
                    selectionChecked = false;
                    return;
                }
                else
                {
                    MapInspector.FocusOn(Selection.activeGameObject, MapInspector.SelectionType.GameObject);
                }
            }
            else
            {
                SetMapObject(Selection.activeGameObject);
                MapInspector.FocusOn(Selection.activeGameObject, MapInspector.SelectionType.GameObject);
            }
        }

        //Debug
        private static bool debug;
        private static List<string> debugMessage = new List<string>();
        public static void Log(string message, int id = 0)
        {
            if (debug) Debug.Log(message);
            if (id >= debugMessage.Count) while (debugMessage.Count <= id) debugMessage.Add("");
            debugMessage[id] = message;
        }
        private void ShowDebugMessage()
        {
            try
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(Prefs.edge + radius * 2);
                GUILayout.BeginVertical();
                debug = GUILayout.Toggle(debug, "Debug Console");
                foreach (string m in debugMessage)
                {
                    GUILayout.Label(m);
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            catch (System.ArgumentException)
            {
                Debug.Log("Layout Problem from MapManager, event type :" + Event.current.type);
                return;
            }
        }




        //UI交互------------------------------------------------------------------------
        private AnimFloat viewScale = new AnimFloat(0.066f);   //预览视图缩放量
        private Quaternion cameraRotation;  //场景相机旋转偏移量
        private float radius;       //圆盘半径
        private Vector2 center;     //圆盘中心

        //辅助配置字段
        private AnimQuaternion targetQuaternion = new AnimQuaternion(Quaternion.Euler(0, MapSystem.currentAngle, 0));  //用于记录CurrentAngle
        private readonly Quaternion rot = Quaternion.Euler(0, 0, -MapSystem.AnglePerGroup);

        private MouseArea mouseArea;    //记录鼠标所处区域
        private int areaIndex;  //记录选中物体的Index，或其他辅助信息

        private AnimFloat mouseDragAngle = new AnimFloat(0);    //记录拖拽旋转角度
        private AnimFloat doubleClickMarkAlpha = new AnimFloat(0);
        private Vector2 doubleClickMarkRawPos;
        private void SetDoubleClickMark()
        {
            doubleClickMarkRawPos = Quaternion.Inverse(cameraRotation) * (Event.current.mousePosition - center);
            doubleClickMarkAlpha.value = 1;
            doubleClickMarkAlpha.target = 0;
        }



        /// <summary>
        /// 鼠标区域位置
        /// </summary>
        private enum MouseArea
        {
            ObjectArea,
            Center,
            ScollArea,
            GroupArea,
            DragArea,
            Invalid,
        }
        /// <summary>
        /// 判定鼠标所处区域
        /// </summary>
        private void SetMouseArea()
        {
            Vector2 mousePos = Event.current.mousePosition;

            //判断顺序=遮挡关系
            //判定选中物体
            MapGroup group = GroupList[MapSystem.currentGroupIndex];
            if (group != null)
                for (int i = 0; i < group.transform.childCount; i++)
                {
                    Vector2 guiPos = GetElementGUIPos(group.transform.GetChild(i).position);

                    if (Vector2.Distance(mousePos, guiPos) < Prefs.elementRadius)
                    {
                        areaIndex = i;
                        mouseArea = MouseArea.ObjectArea;
                        return;
                    }
                }
            for (int i = 0; i < invalidObjectList.Count; i++)
            {
                Vector2 guiPos = GetElementGUIPos(invalidObjectList[i].transform.position);

                if (Vector2.Distance(mousePos, guiPos) < Prefs.elementRadius)
                {
                    areaIndex = -i - 1;
                    mouseArea = MouseArea.ObjectArea;
                    return;
                }
            }
            //判定在中心
            if (Vector2.Distance(mousePos, center) < Prefs.centerDiscRadius)
            {
                mouseArea = MouseArea.Center;
                return;
            }
            //判定在缩放区域
            if (mousePos.x < Prefs.sideWidth)
            {
                mouseArea = MouseArea.ScollArea;
                return;
            }
            //判定选中组
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
            if (group == null) return;
            float angle = group.index * MapSystem.AnglePerGroup + MapSystem.AnglePerGroup / 2;
            for (int i = 0; i < group.transform.childCount; i++)
            {
                Transform child = group.transform.GetChild(i);
                Handles.color = Color.Lerp(Prefs.backColor, Color.Lerp(GetElementColor(child.gameObject), Prefs.circleGradient.Evaluate(angle / MapSystem.MaxAngle), Prefs.elementColorMixRate), alpha);
                Handles.DrawSolidDisc(GetElementGUIPos(child.position), Vector3.back, Prefs.elementRadius);
                Handles.Label(GetElementGUIPos(child.position), child.name, Prefs.elementNameStyle);
            }
            Handles.color = Color.white;

        }
        /// <summary>
        /// 获取物体应该显示的颜色
        /// </summary>
        private Color GetElementColor(GameObject go)
        {
            if (invalidObjectList.Contains(go)) return Prefs.elementInvalidColor;
            MapUnit unit = go.GetComponent<MapUnit>();
            if (unit == null) return Prefs.elementNormalColor;
            else return Color.green;
        }
        /// <summary>
        /// 获取物体应该显示的GUI位置
        /// </summary>
        private Vector2 GetElementGUIPos(Vector3 worldPos)
        {
            return center + (Vector2)(cameraRotation * new Vector2(worldPos.x, -worldPos.z) * viewScale.value * radius);
        }
        /// <summary>
        /// 获取物体世界中位置
        /// </summary>
        public Vector3 GetElementWorldPos(Vector2 guiPos, float height)
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

        private void OnGUI()
        {
            //激活检测
            if (!Active)
            {
                GUILayout.Label("地图没打开，地图编辑器未激活！");
                if (GUILayout.Button("打开地图")) EditorSceneManager.OpenScene("Assets/Scenes/In Game.unity");
                return;
            }

            if (!selectionChecked) CheckSelection();

            //场景参数计算
            radius = Mathf.Min(EditorGUIUtility.currentViewWidth - Prefs.sideWidth, position.height) / 2 - Prefs.edge;
            center = new Vector2(radius + Prefs.edge + Prefs.sideWidth, radius + Prefs.edge);

            Vector3 cameraDir = SceneView.lastActiveSceneView == null ? Vector3.back : SceneView.lastActiveSceneView.camera.transform.forward;
            Vector2 start = Vector3.right;
            cameraRotation = Quaternion.LookRotation(Vector3.forward, new Vector3(cameraDir.x, cameraDir.z, 0));
            Vector2 mid = Quaternion.Euler(0, 0, -MapSystem.AnglePerGroup / 2) * Vector2.right * radius / 2;

            //事件交互
            switch (Event.current.type)
            {
                case EventType.Layout:
                case EventType.Repaint:
                    {
                        //画底部圆盘
                        for (int i = 0; i < 3; i++)
                        {
                            float angle = MapSystem.GetAngle(new Vector3(mid.x, 0, -mid.y));
                            int index = (int)(angle / MapSystem.AnglePerGroup);

                            Color color;
                            if (GroupList[index] == null) color = Color.gray;
                            else color = Prefs.circleGradient.Evaluate(angle / MapSystem.MaxAngle);

                            float alpha = 1 - Prefs.arcAlphaCurve.Evaluate(Mathf.Abs(MapSystem.SubSigned(MapSystem.currentAngle, angle)));
                            color.a = Mathf.Lerp(0, 1, alpha);
                            Handles.color = color;
                            Handles.DrawSolidArc((Vector3)center + cameraRotation * mid * Mathf.Lerp(Prefs.arcCenterOffsetRateMin, Prefs.arcCenterOffsetRateMax, alpha), Vector3.back, cameraRotation * start, 120, (radius + Mathf.Lerp(-10, 25, (viewScale.value - Prefs.minScale) / (Prefs.maxScale - Prefs.minScale))) * Mathf.Lerp(Prefs.arcMinRadiusRate, 1, alpha));
                            if (GroupList[index] == null) Handles.Label((Vector3)center + cameraRotation * mid * Mathf.Lerp(1, Prefs.groupNumDistance, alpha), index.ToString(), Prefs.groupNumStyle);
                            else Handles.Label((Vector3)center + cameraRotation * mid * Mathf.Lerp(1, Prefs.groupNumDistance, alpha), index.ToString() + " " + GroupList[index].groupName + (GroupList[index].dirty ? "*" : ""), Prefs.groupNumStyle);
                            start = rot * start;
                            mid = rot * mid;
                        }
                        Handles.color = Color.white;

                        //中心圆盘
                        Handles.color = Prefs.centerDiscColor;
                        {
                            Handles.DrawSolidDisc(center, Vector3.back, Prefs.centerDiscRadius);
                            Handles.Label(center - Prefs.centerDiscStyle.CalcSize(new GUIContent(MapSystem.CurrentCircle.ToString())) / 2f, MapSystem.CurrentCircle.ToString(), Prefs.centerDiscStyle);
                        }
                        Handles.color = Color.white;


                        //画侧面刻度
                        Handles.color = Prefs.sideColor;
                        {
                            Vector2 yRange = new Vector2(Prefs.edge + (1 - Prefs.sideLineLengthRate) * radius, Prefs.edge + (1 + Prefs.sideLineLengthRate) * radius);
                            Vector2 xRange = new Vector2(Prefs.sideWidth - Prefs.sideXOffset + (Prefs.sideFlip ? -Prefs.sideMarkWidth : Prefs.sideMarkWidth), Prefs.sideWidth - Prefs.sideXOffset);
                            Handles.DrawAAPolyLine(
                                Prefs.sideLineWidth,
                                new Vector2(xRange.x, yRange.x),
                                new Vector2(xRange.y, yRange.x),
                                new Vector2(xRange.y, yRange.y),
                                new Vector2(xRange.x, yRange.y)
                                );

                            xRange.x = Prefs.sideWidth - Prefs.sideXOffset + (Prefs.sideFlip ? -Prefs.sideMarkWidth * Prefs.sideLineLengthRate : Prefs.sideMarkWidth * Prefs.sideLineLengthRate);
                            for (int i = 1; i < Prefs.sideMarkNum; i++)
                            {
                                float yPos = Mathf.Lerp(yRange.x, yRange.y, (float)i / (float)Prefs.sideMarkNum);
                                Handles.DrawLine(new Vector2(xRange.x, yPos), new Vector2(xRange.y, yPos));
                            }

                            float arrowXOffset = (Prefs.sideFlip ? -1 : 1) * Prefs.sideArrowSize * Mathf.Cos(Prefs.sideArrowAngle * Mathf.PI / 180f);
                            float arrowyPos = Mathf.Lerp(yRange.y, yRange.x, (viewScale.value - Prefs.minScale) / (Prefs.maxScale - Prefs.minScale));
                            Handles.ArrowHandleCap(0, new Vector2(xRange.x + arrowXOffset, arrowyPos), Quaternion.Euler(0, (Prefs.sideFlip ? 90 : -90) + Prefs.sideArrowAngle, 0), Prefs.sideArrowSize, EventType.Repaint);
                            Handles.Label(new Vector2(xRange.x + arrowXOffset / 2, arrowyPos), "缩放等级", Prefs.sideMarkStyle);
                        }
                        Handles.color = Color.white;

                        //画双击指示
                        if (doubleClickMarkAlpha.value > 0)
                        {
                            Handles.color = new Color(1, 1, 1, doubleClickMarkAlpha.value);
                            Handles.DrawSolidDisc(cameraRotation * doubleClickMarkRawPos + (Vector3)center, Vector3.back, (1 - doubleClickMarkAlpha.value) * Prefs.doubleClickMarkRadius);
                            Handles.color = Color.white;
                        }

                        //画元素
                        Handles.color = Prefs.elementOutLineColor;
                        {
                            foreach (GameObject g in Selection.gameObjects)
                            {
                                PrefabType prefabType = PrefabUtility.GetPrefabType(g);
                                if (prefabType != PrefabType.Prefab)
                                {
                                    Handles.DrawSolidDisc(GetElementGUIPos(g.transform.position), Vector3.back, Prefs.elementRadius + Prefs.elementOutlineWidth);
                                }
                            }
                            foreach (GameObject g in invalidObjectList)
                            {
                                Handles.color = GetElementColor(g);
                                Handles.DrawSolidDisc(GetElementGUIPos(g.transform.position), Vector3.back, Prefs.elementRadius);
                            }

                            MapGroup group = GroupList[MapSystem.currentGroupIndex];
                            DrawElement(group, 1);

                            group = GroupList[MapSystem.GetPrevious(MapSystem.currentGroupIndex)];
                            DrawElement(group, Prefs.elementAlpha);

                            group = GroupList[MapSystem.GetNext(MapSystem.currentGroupIndex)];
                            DrawElement(group, Prefs.elementAlpha);
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
                            MapInspector.FocusOn(GroupList[areaIndex], MapInspector.SelectionType.Group);

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
                            GameObject objectSelected = areaIndex >= 0 ? GroupList[MapSystem.currentGroupIndex].transform.GetChild(areaIndex).gameObject : invalidObjectList[-areaIndex - 1];
                            Selection.activeGameObject = objectSelected;

                            break;
                        case MouseArea.Invalid:
                        case MouseArea.DragArea:
                        case MouseArea.Center:
                            //选中map
                            Selection.activeGameObject = null;
                            MapInspector.FocusOn(null, MapInspector.SelectionType.Map);
                            break;
                    }
                    break;
                case EventType.MouseDrag:
                    if (Event.current.button == 1)
                    {
                        //right click
                        SceneView.lastActiveSceneView.rotation = Quaternion.Euler(0, Event.current.delta.x / 2, 0) * SceneView.lastActiveSceneView.rotation * Quaternion.Euler(Event.current.delta.y / 2, 0, 0);
                    }
                    else switch (mouseArea)
                        {
                            case MouseArea.ScollArea:
                            case MouseArea.GroupArea:
                            case MouseArea.DragArea:
                                Vector3 mouseDir = Event.current.mousePosition - center;
                                float mouseDirSqrLength = mouseDir.sqrMagnitude;
                                float dragAngle = -Vector3.Cross(mouseDir, Event.current.delta).z / mouseDirSqrLength * 180 / Mathf.PI * Prefs.dragSensitivity;

                                mouseDragAngle.value = dragAngle;
                                mouseDragAngle.target = 0;

                                break;
                            case MouseArea.ObjectArea:
                                Undo.RecordObject(Selection.activeGameObject.transform, "Moving Object");
                                Selection.activeGameObject.transform.position = GetElementWorldPos(Event.current.mousePosition, Selection.activeGameObject.transform.position.y);
                                break;
                        }
                    break;
                case EventType.MouseUp:
                    switch (mouseArea)
                    {
                        case MouseArea.ObjectArea:
                            SetMapObject(Selection.activeGameObject);
                            break;
                    }
                    break;
                case EventType.ScrollWheel:
                    viewScale.target = Mathf.Clamp(viewScale.target - Event.current.delta.y * Prefs.scaleSensitivity, Prefs.minScale, Prefs.maxScale);
                    break;
            }

            //Debug Message
            ShowDebugMessage();
        }





        //场景编辑方法-------------------------------------------------------------------
        private static List<GameObject> invalidObjectList = new List<GameObject>();
        /// <summary>
        /// 将GameObject设为对应Group的子物体
        /// </summary>
        public static void SetMapObject(GameObject g)
        {
            if (g == null || !g.CompareTag("Untagged")) return;
            if (invalidObjectList.Contains(g)) invalidObjectList.Remove(g);
            MapGroup group = g.GetComponentInParent<MapGroup>();
            float gAngle = MapSystem.GetAngle(g.transform.position);
            int gIndex = (int)(gAngle / MapSystem.AnglePerGroup);
            MapUnit[] units = g.GetComponentsInChildren<MapUnit>();

            if (group == null)
            {
                //新添加
                Log("Adding " + g);
                MapGroup newGroup = GroupList[gIndex];
                if (newGroup == null)
                {
                    Debug.LogAssertion("物体被拖进了未知领域!");
                    invalidObjectList.Add(g);
                    return;
                }
                g.transform.SetParent(newGroup.transform, true);

                foreach (MapUnit unit in units)
                {
                    AddUnit(unit, newGroup);
                }

                newGroup.dirty = true;
            }
            else if (gIndex != group.index)
            {
                //换组
                Log("Change " + g);
                MapGroup newGroup = GroupList[gIndex];
                if (newGroup == null)
                {
                    Debug.LogAssertion("物体被拖进了未知领域!");
                    invalidObjectList.Add(g);
                    return;
                }
                g.transform.SetParent(newGroup.transform, true);

                foreach (MapUnit unit in units)
                {
                    DeleteUnit(unit);
                    AddUnit(unit, newGroup);
                }
                group.dirty = true;
                newGroup.dirty = true;
            }
            else
            {
                //调整
                Log("Adjust " + g);
                foreach (MapUnit unit in units)
                {
                    AdjustUnit(unit);
                }
                group.dirty = true;
            }
        }
        //这三个方法的核心是保证升序排列
        private static void AddUnit(MapUnit unit, MapGroup group)
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

            group.dirty = true;
        }
        private static void DeleteUnit(MapUnit unit)
        {
            MapGroup group = unit.group;

            group.unitList.RemoveAt(unit.index);
            unit.group = null;

            for (int j = unit.index; j < group.unitList.Count; j++)
            {
                group.unitList[j].index = j;
            }
            group.dirty = true;
        }
        private static void AdjustUnit(MapUnit unit)
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
