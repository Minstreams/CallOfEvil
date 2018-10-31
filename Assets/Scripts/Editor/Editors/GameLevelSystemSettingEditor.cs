using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GameSystem;
using GameSystem.PresentSetting;


[CustomEditor(typeof(GameLevelSystemSetting))]
public class GameLevelSystemSettingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUILayout.Label("最大玩家人数：" + GameLevelSystem.MaxPlayerCount);
        base.OnInspectorGUI();
    }
}
