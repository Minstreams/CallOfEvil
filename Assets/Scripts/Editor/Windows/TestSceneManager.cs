﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.AnimatedValues;
using GameSystem;


/// <summary>
/// 测试场景管理器，用于自动配好测试环境
/// </summary>
public class TestSceneManager : EditorWindow
{
    [Header("场景测试器，用于自动配好测试环境")]
    public SceneAsset _system;
    public SceneAsset _loading;
    public SceneAsset _logo;
    public SceneAsset _startMenu;
    public SceneAsset _lobby;
    public SceneAsset _inGame;


    AnimBool fade0 = new AnimBool(false);
    string console;
    bool quickStart;
    private void OnGUI()
    {
        fade0.target = EditorGUILayout.Toggle("场景配置", fade0.target);
        bool show = EditorGUILayout.BeginFadeGroup(fade0.faded);
        if (show)
        {
            _system = EditorGUILayout.ObjectField("System", _system, typeof(SceneAsset), false) as SceneAsset;
            _loading = EditorGUILayout.ObjectField("_loading", _loading, typeof(SceneAsset), false) as SceneAsset;
            _logo = EditorGUILayout.ObjectField("_logo", _logo, typeof(SceneAsset), false) as SceneAsset;
            _startMenu = EditorGUILayout.ObjectField("_startMenu", _startMenu, typeof(SceneAsset), false) as SceneAsset;
            _lobby = EditorGUILayout.ObjectField("_lobby", _lobby, typeof(SceneAsset), false) as SceneAsset;
            _inGame = EditorGUILayout.ObjectField("_inGame", _inGame, typeof(SceneAsset), false) as SceneAsset;
        }
        EditorGUILayout.EndFadeGroup();

        quickStart = EditorGUILayout.Toggle("自动开始", quickStart);

        if (GUILayout.Button("完整测试")) SetTestScene();
        if (GUILayout.Button("单独测试")) SetTestSceneSingle();

        EditorGUILayout.LabelField(console);
    }

    [MenuItem("自制工具/场景测试器 _F5")]
    static void OpenWindow()
    {
        TestSceneManager window = EditorWindow.GetWindow<TestSceneManager>("【Tester】");
    }

    private void Awake()
    {
        EditorSystem.EditorMatrix.Load(this);
        fade0.valueChanged.AddListener(() => Repaint());
    }

    private void OnDestroy() { EditorSystem.EditorMatrix.Save(this); }


    private void Log(string text)
    {
        Debug.Log(text);
        console = text;
    }

    private void LogError(string text)
    {
        Debug.LogError(text);
        console = text;
    }

    public void SetTestSceneSingle()
    {
        if (!(_system || _logo || _startMenu || _lobby || _inGame))
        {
            LogError("有场景未指定！无法加载测试环境！");
            return;
        }
        //保存场景
        EditorSceneManager.SaveOpenScenes();

        //激活场景重定向
        GameObject ag = Selection.activeGameObject;
        if (ag != null) EditorSceneManager.SetActiveScene(ag.scene);

        //删除多余场景
        Scene activeScene = EditorSceneManager.GetActiveScene();
        for (int i = EditorSceneManager.sceneCount - 1; i >= 0; i--)
        {
            Scene s = EditorSceneManager.GetSceneAt(i);
            if (s != activeScene)
                EditorSceneManager.CloseScene(s, true);
        }

        //加载system场景并配置The Matrix
        Scene systemScene =
        EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(_system), OpenSceneMode.Additive);
        systemScene.GetRootGameObjects()[0].GetComponent<TheMatrix>().test = false;

        EditorSceneManager.SaveScene(systemScene);

        if (quickStart) EditorApplication.isPlaying = true;
    }

    public void SetTestScene()
    {
        if (!(_system || _logo || _startMenu || _lobby || _inGame))
        {
            LogError("有场景未指定！无法加载测试环境！");
            return;
        }
        //保存场景
        EditorSceneManager.SaveOpenScenes();

        //加载system场景并配置The Matrix
        Scene systemScene =
        EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(_system), OpenSceneMode.Single);

        TheMatrix matrix = systemScene.GetRootGameObjects()[0].GetComponent<TheMatrix>();
        matrix._logo = _logo.name;
        matrix._startMenu = _startMenu.name;
        matrix._lobby = _lobby.name;
        matrix._inGame = _inGame.name;
        matrix.test = true;

        EditorSceneManager.SaveScene(systemScene);

        //加载Loading场景并配置SceneSystem
        EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(_loading), OpenSceneMode.AdditiveWithoutLoading);
        GameSystem.PresentSetting.SceneSystemSetting sceneSystemSetting = Resources.Load<GameSystem.PresentSetting.SceneSystemSetting>("System/" + typeof(GameSystem.PresentSetting.SceneSystemSetting).ToString().Substring(26));
        sceneSystemSetting.loadingScene = _loading.name;

        //加载其它场景
        EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(_logo), OpenSceneMode.AdditiveWithoutLoading);
        EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(_startMenu), OpenSceneMode.AdditiveWithoutLoading);
        EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(_lobby), OpenSceneMode.AdditiveWithoutLoading);
        EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(_inGame), OpenSceneMode.AdditiveWithoutLoading);

        Log("测试环境准备完毕！");

        if (quickStart) EditorApplication.isPlaying = true;
    }
}