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

        public static void Save(Object data)
        {
            string stream = JsonUtility.ToJson(data);
            EditorPrefs.SetString(data.GetType().ToString(), stream);
            Debug.Log(data.name + " \teditor saved!");
        }
        public static void Load(Object data)
        {
            if (!EditorPrefs.HasKey(data.GetType().ToString()))
            {
                Debug.Log("No editor data found for " + data.name);
                return;
            }
            string stream = EditorPrefs.GetString(data.GetType().ToString());
            JsonUtility.FromJsonOverwrite(stream, data);
            Debug.Log(data.name + " \teditor loaded!");
        }
        [ContextMenu("Delete All editor Data")]
        public static void DeleteAll()
        {
            EditorPrefs.DeleteAll();
            Debug.Log("All saved editor data deleted!");
        }
    }
}
