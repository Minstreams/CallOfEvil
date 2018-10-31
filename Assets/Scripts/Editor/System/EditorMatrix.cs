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
        private static void SaveKeys()
        {
            string stream = JsonUtility.ToJson(keys);
            EditorPrefs.SetString("keys", stream);
        }
        private static void SaveKeys(string key)
        {
            if (!keys.Contains(key))
            {
                keys.Add(key);
                SaveKeys();
            }
        }
        [InitializeOnLoadMethod]
        private static void LoadKeys()
        {
            if (!EditorPrefs.HasKey("keys"))
                return;

            string stream = EditorPrefs.GetString("keys");
            JsonUtility.FromJsonOverwrite(stream, keys);
        }


        /// <summary>
        /// 保存编辑器数据，数据按类型区分
        /// </summary>
        public static void Save(Object data)
        {
            SaveKeys(data.GetType().ToString());

            string stream = JsonUtility.ToJson(data);
            EditorPrefs.SetString(data.GetType().ToString(), stream);
            Debug.Log(data + " \teditor saved!");
        }
        /// <summary>
        /// 载入编辑器数据，数据按类型区分
        /// </summary>
        public static void Load(Object data)
        {
            if (!EditorPrefs.HasKey(data.GetType().ToString()))
            {
                Debug.Log("No editor data found for " + data.GetType().ToString());
                return;
            }
            SaveKeys(data.GetType().ToString());

            string stream = EditorPrefs.GetString(data.GetType().ToString());
            JsonUtility.FromJsonOverwrite(stream, data);
            Debug.Log(data + " \teditor loaded!");
        }
        [MenuItem("开发者工具/Delete All editor Data")]
        public static void DeleteAll()
        {
            //EditorPrefs.DeleteAll();  //直接deleteAll会把系统设置也Delete掉
            foreach (string k in keys)
            {
                EditorPrefs.DeleteKey(k);
            }
            keys.Clear();
            SaveKeys();

            Debug.Log("All saved editor data deleted!");
        }
    }

    public class SavablePref : Object
    {
        public void Load() { EditorMatrix.Load(this); }
        public void Save() { EditorMatrix.Save(this); }
    }

}
