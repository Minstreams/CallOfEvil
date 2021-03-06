﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.AnimatedValues;
using GameSystem;


namespace EditorSystem
{
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

        public class SceneAssetPaths
        {
            public string _system;
            public string _loading;
            public string _logo;
            public string _startMenu;
            public string _lobby;
            public string _inGame;

            public SceneAssetPaths(string _0, string _1, string _2, string _3, string _4, string _5)
            {
                _system = _0;
                _loading = _1;
                _logo = _2;
                _startMenu = _3;
                _lobby = _4;
                _inGame = _5;
            }
        }

        SceneAssetPaths paths = new SceneAssetPaths(
            "Assets/Scenes/System.unity",
            "Assets/Scenes/Loading.unity",
            "Assets/Scenes/Logo.unity",
            "Assets/Scenes/StartMenu.unity",
            "Assets/Scenes/Lobby.unity",
            "Assets/Scenes/In Game.unity");  //存储场景路径

        ReadmeGUIStyles Styles
        {
            get
            {
                if (m_Styles == null) m_Styles = (ReadmeGUIStyles)EditorGUIUtility.Load("Styles/Default.asset");
                return m_Styles;
            }
        }
        ReadmeGUIStyles m_Styles = null;

        AnimBool fade0 = new AnimBool(false);
        string console;
        [SerializeField]
        bool quickStart;
        private void OnGUI()
        {
            GUILayout.BeginHorizontal(Styles.header);
            GUILayout.Label("岷溪的场景测试器", Styles.title, GUILayout.ExpandHeight(false));
            GUILayout.EndHorizontal();

            fade0.target = EditorGUILayout.Toggle("场景配置", fade0.target);
            bool show = EditorGUILayout.BeginFadeGroup(fade0.faded);
            if (show)
            {
                GUILayout.BeginVertical(Styles.section);
                _system = EditorGUILayout.ObjectField("System", _system, typeof(SceneAsset), false) as SceneAsset;
                _loading = EditorGUILayout.ObjectField("_loading", _loading, typeof(SceneAsset), false) as SceneAsset;
                _logo = EditorGUILayout.ObjectField("_logo", _logo, typeof(SceneAsset), false) as SceneAsset;
                _startMenu = EditorGUILayout.ObjectField("_startMenu", _startMenu, typeof(SceneAsset), false) as SceneAsset;
                _lobby = EditorGUILayout.ObjectField("_lobby", _lobby, typeof(SceneAsset), false) as SceneAsset;
                _inGame = EditorGUILayout.ObjectField("_inGame", _inGame, typeof(SceneAsset), false) as SceneAsset;
                GUILayout.EndVertical();
            }
            EditorGUILayout.EndFadeGroup();

            quickStart = EditorGUILayout.Toggle("自动开始", quickStart);


            GUILayout.BeginHorizontal(Styles.header, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true));
            if (GUILayout.Button("完整测试", Styles.button)) SetTestScene();
            if (GUILayout.Button("单独测试", Styles.button)) SetTestSceneSingle();
            GUILayout.EndHorizontal();

            GUILayout.Label(console, Styles.text);
        }

        [MenuItem("自制工具/测试器 _F5")]
        static void OpenWindow()
        {
            EditorWindow.GetWindow<TestSceneManager>("测试器");
        }

        private void Awake()
        {
            EditorSystem.EditorMatrix.Load(this);
            LoadPaths();
            fade0.valueChanged.AddListener(() => Repaint());
        }
        private void OnDestroy()
        {
            SavePaths();
            EditorSystem.EditorMatrix.Save(this);
        }

        private void SavePaths()
        {
            paths._system = AssetDatabase.GetAssetPath(_system);
            paths._loading = AssetDatabase.GetAssetPath(_loading);
            paths._logo = AssetDatabase.GetAssetPath(_logo);
            paths._startMenu = AssetDatabase.GetAssetPath(_startMenu);
            paths._lobby = AssetDatabase.GetAssetPath(_lobby);
            paths._inGame = AssetDatabase.GetAssetPath(_inGame);
        }
        private void LoadPaths()
        {
            _system = AssetDatabase.LoadAssetAtPath<SceneAsset>(paths._system);
            _loading = AssetDatabase.LoadAssetAtPath<SceneAsset>(paths._loading);
            _logo = AssetDatabase.LoadAssetAtPath<SceneAsset>(paths._logo);
            _startMenu = AssetDatabase.LoadAssetAtPath<SceneAsset>(paths._startMenu);
            _lobby = AssetDatabase.LoadAssetAtPath<SceneAsset>(paths._lobby);
            _inGame = AssetDatabase.LoadAssetAtPath<SceneAsset>(paths._inGame);
        }

        private bool CheckSceneAsset(SceneAsset asset)
        {
            return AssetDatabase.GetAssetPath(asset).Contains("Scenes");
        }
        private bool CheckProperties()
        {
            if (!(_system || _logo || _startMenu || _lobby || _inGame))
            {
                LogError("有场景未指定！无法加载测试环境！");
                return false;
            }
            if (!(CheckSceneAsset(_system) &&
                CheckSceneAsset(_loading) &&
                CheckSceneAsset(_logo) &&
                CheckSceneAsset(_startMenu) &&
                CheckSceneAsset(_lobby) &&
                CheckSceneAsset(_inGame)))
            {
                LogError("场景有问题！请重新指定场景！");
                return false;
            }
            return true;
        }
        public void SetTestSceneSingle()
        {
            if (!CheckProperties()) return;
            //保存场景
            EditorSceneManager.SaveOpenScenes();

            //激活场景重定向
            GameObject ag = Selection.activeGameObject;
            if (ag != null) EditorSceneManager.SetActiveScene(ag.scene);

            //删除多余场景
            Scene activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.name == _system.name)
            {
                LogError("请选中除了System以外的任一场景（中的物体）");
                return;
            }
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

            EditorSceneManager.SetActiveScene(activeScene);
            EditorSceneManager.SaveScene(systemScene);

            Log("单独测试环境准备完毕！");

            if (quickStart) EditorApplication.isPlaying = true;
        }
        public void SetTestScene()
        {
            if (EditorApplication.isPlaying)
            {
                LogError("请先退出播放模式！");
                return;
            }
            if (!CheckProperties()) return;
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
    }
}