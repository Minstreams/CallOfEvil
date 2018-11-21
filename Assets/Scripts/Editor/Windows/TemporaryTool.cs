using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace EditorSystem
{
    /// <summary>
    /// 暂时工具集，一些小的功能
    /// </summary>
    public class TemporaryTool : EditorWindow
    {
        [MenuItem("开发者工具/小工具 _F8")]
        static void OpenWindow()
        {
            EditorWindow.GetWindow<TemporaryTool>("小工具", true);
        }


        //复制数据，利用反射机制-------------------------------------------
        class PropertyReflectionInformation
        {
            public Object obj;
            public System.Type type { get { return obj.GetType(); } }
            public Editor editor;
            public Stack<FieldInfo[]> fieldStack = new Stack<FieldInfo[]>();
            public List<string> popupNames = new List<string>();
            public int selection;

            public FieldInfo GetField() { return type.GetField(popupNames[selection]); }
        }

        PropertyReflectionInformation source = new PropertyReflectionInformation();
        PropertyReflectionInformation destination = new PropertyReflectionInformation();

        void CopyDataGUI()
        {
            GUILayout.Label("【复制数据】", "In BigTitle", GUILayout.ExpandWidth(true));


            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUI.BeginChangeCheck();
                source.obj = EditorGUILayout.ObjectField("源", source.obj, typeof(Object), false);
                if (EditorGUI.EndChangeCheck())
                {
                    source.popupNames.Clear();
                    source.selection = 0;
                    if (source.obj == null)
                    {
                        source.fieldStack.Clear();
                    }
                    else
                    {
                        source.fieldStack.Clear();
                        source.fieldStack.Push(source.type.GetFields());
                        foreach (FieldInfo f in source.fieldStack.Peek()) source.popupNames.Add(f.Name);
                    }
                }
                if (source.popupNames.Count > 0)
                {
                    source.selection = EditorGUILayout.Popup(source.selection, source.popupNames.ToArray());
                    if (GUILayout.Button("Next")) source.selection++;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                destination.obj = EditorGUILayout.ObjectField("目标", destination.obj, typeof(Object), false);
                if (EditorGUI.EndChangeCheck())
                {
                    destination.popupNames.Clear();
                    destination.selection = 0;

                    if (destination.obj == null)
                    {
                        destination.fieldStack.Clear();
                    }
                    else
                    {
                        destination.fieldStack.Clear();
                        destination.fieldStack.Push(destination.type.GetFields());
                        if (source.popupNames.Count > 0)
                        {
                            FieldInfo sourceField = source.GetField();

                            foreach (FieldInfo f in destination.fieldStack.Peek())
                            {
                                if (f.FieldType == sourceField.FieldType)
                                {
                                    if (f.Name == sourceField.Name)
                                    {
                                        destination.selection = destination.popupNames.Count;
                                    }
                                    destination.popupNames.Add(f.Name);
                                }
                            }
                        }
                    }
                }
                if (destination.popupNames.Count > 0) destination.selection = EditorGUILayout.Popup(destination.selection, destination.popupNames.ToArray());
            }
            EditorGUILayout.EndHorizontal();

            if (source.popupNames.Count > 0 && destination.popupNames.Count > 0 && GUILayout.Button("复制！")) CopyData();
        }

        void CopyData()
        {
            Undo.RecordObject(destination.obj, "复制,From " + source.obj.name + " to " + destination.obj.name);

            //获取信息
            FieldInfo fromField = source.GetField();
            FieldInfo toField = destination.GetField();

            System.Type type = fromField.FieldType;

            if (type == typeof(int) || type == typeof(float) || type == typeof(Color) || type == typeof(bool))
            {
                toField.SetValue(destination.obj, fromField.GetValue(source.obj));
            }
            else if (type == typeof(GUIStyle))
            {
                System.Type[] constructorTypes = { type };
                object[] constructorParams = { fromField.GetValue(source.obj) };
                toField.SetValue(destination.obj, type.GetConstructor(constructorTypes).Invoke(constructorParams));
            }
            else if (type == typeof(AnimationCurve))
            {
                AnimationCurve fromC = fromField.GetValue(source.obj) as AnimationCurve;
                Keyframe[] keys = fromC.keys;
                List<Keyframe> keyList = new List<Keyframe>();
                foreach (Keyframe k in keys)
                {
                    keyList.Add(new Keyframe(k.time, k.value, k.inTangent, k.outTangent));
                }
                toField.SetValue(destination.obj, new AnimationCurve(keyList.ToArray()));
            }
            else if (type == typeof(Gradient))
            {
                Gradient fromG = fromField.GetValue(source.obj) as Gradient;
                Gradient value = new Gradient();
                GradientAlphaKey[] aKeys = new GradientAlphaKey[fromG.alphaKeys.Length];
                GradientColorKey[] cKeys = new GradientColorKey[fromG.colorKeys.Length];
                for (int i = 0; i < fromG.alphaKeys.Length; i++) aKeys[i] = fromG.alphaKeys[i];
                for (int i = 0; i < fromG.colorKeys.Length; i++) cKeys[i] = fromG.colorKeys[i];
                value.SetKeys(cKeys, aKeys);
                toField.SetValue(destination.obj, value);
            }
            else
            {
                object so = fromField.GetValue(source.obj);
                foreach (FieldInfo f in type.GetFields())
                {
                    Debug.Log("FieldType:" + f.FieldType + ";value:" + f.GetValue(so));
                }
                Debug.LogAssertion("不支持的类型！ type:" + type + " ; from " + fromField.Name + " to " + toField.Name);
                return;
            }
            Debug.Log("复制成功！ type:" + type + " ; from " + fromField.Name + " to " + toField.Name);
        }


        //卸载场上失效场景组-----------------------------------------------
        [MenuItem("开发者工具/重新组织场景")]
        public static void RearrangeInvalidMapGroup()
        {
            if (!GameSystem.MapSystem.Active)
            {
                Debug.Log("请先加载地图场景！");
                return;
            }
            MapGroup[] groups = Resources.FindObjectsOfTypeAll<MapGroup>();
            Debug.Log(groups.Length + " Groups Found!");
            List<MapGroup> groupList = GameSystem.MapSystem.groupList;
            foreach (MapGroup mg in groups)
            {
                if (PrefabUtility.GetPrefabType(mg) == PrefabType.Prefab) continue;
                if (mg.index < 0 || mg.index >= groupList.Count || (groupList[mg.index] != null && groupList[mg.index] != mg))
                {
                    Debug.Log("Delete " + mg.groupName);
                    GameSystem.MapSystem.UnLoadGroup(mg);
                    continue;
                }
                else if (groupList[mg.index] == null)
                {
                    Debug.Log("Rearrange " + mg.groupName + " to " + mg.index);
                    groupList[mg.index] = mg;
                }
            }
            GameSystem.MapSystem.InitGroupActiveState();
            Debug.Log("Done.");
        }

        private void OnGUI()
        {
            CopyDataGUI();
        }

    }
}
