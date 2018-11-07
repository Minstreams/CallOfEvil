using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EditorSystem
{
    /// <summary>
    /// 代替Assets的资源窗口
    /// </summary>
    public class MapAssetManager : EditorWindow
    {
        //引用
        private static MapManagerStyle Style { get { return MapManager.Style; } }

        private int toolbarSelected = 0;
        private readonly string[] toolbarTitle = { "物体", "地图组预设" };

        private void OnGUI()
        {
            toolbarSelected = GUILayout.Toolbar(toolbarSelected, toolbarTitle, Style.toolbarStyle);

            if (GUILayout.Button("Test！")) Test();
        }

        public void Test()
        {
            Debug.Log("Test!");
        }
    }
}
