using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GameSystem;
using GameSystem.PresentSetting;

[CustomEditor(typeof(MapSystemSetting))]
public class MapSystemSettingEditor : Editor {

    public override void OnInspectorGUI()
    {
        GUILayout.Label(MapSystem.地图生成方案挑选方式说明);
        base.OnInspectorGUI();
    }
}
