using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EditorSystem
{
    /// <summary>
    /// 编辑器的母体
    /// </summary>
    public static class EditorMatrix
    {
        static List<string> keys = new List<string>();
        public static void Save(Object data)
        {
            keys.Add(data.GetType().ToString());

            string stream = JsonUtility.ToJson(data);
            EditorPrefs.SetString(data.GetType().ToString(), stream);
            Debug.Log(data + " \teditor saved!");
        }
        public static void Load(Object data)
        {
            if (!EditorPrefs.HasKey(data.GetType().ToString()))
            {
                Debug.Log("No editor data found for " + data);
                return;
            }
            keys.Add(data.GetType().ToString());

            string stream = EditorPrefs.GetString(data.GetType().ToString());
            JsonUtility.FromJsonOverwrite(stream, data);
            Debug.Log(data + " \teditor loaded!");
        }
        [MenuItem("开发者工具/Delete All editor Data")]
        public static void DeleteAll()
        {
            //EditorPrefs.DeleteAll();
            foreach(string k in keys)
            {
                EditorPrefs.DeleteKey(k);
            }
            keys.Clear();
            Debug.Log("All saved editor data deleted!");
        }
    }
}
