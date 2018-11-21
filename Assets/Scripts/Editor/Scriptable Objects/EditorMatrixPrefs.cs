using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EditorSystem
{
    [CreateAssetMenu(fileName = "Editor Matrix Prefs", menuName = "系统配置文件/Editor Matrix Prefs")]
    public class EditorMatrixPrefs : ScriptableObject
    {

        [Header("场景GizmoStyle")]
        public Color arcColor;
        public GUIStyle circleIndexStyle;
        public GUIStyle angleStyle;
        public float arcRadius = 20;


        public AnimationCurve circleVerticalCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(180f, 1f, 0f, 0f));
        public AnimationCurve circleColorCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(180f, 1f, 0f, 0f));
        public float circleVerticleRate = 1;
        public float circleWidth = 4;
        public Gradient circleGradient = new Gradient();
        public Color backColor;
        public float r0 = 1;
        public float r2 = 1.1f;
        public float r3 = 1.3f;

        public float arrowSize = 0.2f;
        public Color arrowColor;

        public float headHeight;
        public float headSize;

        public float outlineWidth = 4;
        public float outlineBegin = 1.1f;
        public float outlineEnd = 10f;


        [Header("编辑器Style")]
        public AnimationCurve arcAlphaCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(180f, 1f, 0f, 0f));
        [Range(0.5f, 1)]
        public float arcMinRadiusRate = 0.85f;
        [Range(-0.5f, 0.5f)]
        public float arcCenterOffsetRateMin = 0.1f;
        [Range(0, 0.6f)]
        public float arcCenterOffsetRateMax = 0.2f;
        public float edge = 20;
        public GUIStyle centerDiscStyle;
        public Color centerDiscColor = Color.black;
        public float centerDiscRadius = 40;
        public GUIStyle groupNumStyle;
        public float groupNumDistance;
        public float elementRadius = 8;
        public float elementOutlineWidth = 8;
        public float minScale = 0.001f;
        public float maxScale = 0.1f;
        public float scaleSensitivity = 0.01f;
        public float dragSensitivity = 0.6f;
        public float elementColorMixRate = 0.5f;
        public Color elementBackColor = Color.clear;
        public Color elementOutLineColor = Color.white;
        public Color elementNormalColor = Color.black;
        public Color elementInvalidColor = Color.yellow;
        public float elementAlpha = 0.5f;
        public GUIStyle elementNameStyle;
        public float sideWidth = 40;
        public float sideMarkWidth = 6;
        public GUIStyle sideMarkStyle;
        public Color sideColor = Color.white;
        public float sideXOffset = 0;
        public int sideMarkNum = 4;
        public float sideLineWidth = 4;
        [Range(0, 1)]
        public float sideLineLengthRate = 1;
        public float sideArrowAngle = 0;
        public float sideArrowSize = 60;
        public bool sideFlip = false;
        public float doubleClickMarkRadius = 20;

        [Header("属性面板Style")]
        public GUIStyle debugMessageStyle;
        public GUIStyle nameStyle;
        public GUIStyle informationStyle;

        [Header("资源面板Style")]
        public GUIStyle toolbarStyle;
        public GUIStyle mapAssetBackgroundStyle;
        public GUIStyle mapAssetIconStyle;
        public GUIStyle mapAssetLabelStyle;
        public string mapAssetUnbakedStateMark = "*";



        [Header("重要数据存储")]
        /// <summary>
        /// 所有地图组预设
        /// </summary>
        public List<MapGroupAsset> mapGroupAssets = new List<MapGroupAsset>();



        /// <summary>
        /// 地图物体信息表
        /// </summary>
        [ContextMenuItem("Clear", "ClearDictionary")]
        [ContextMenuItem("Clear Nullptr", "ClearDictionaryNullptr")]
        public PrefabDictionary prefabDictionary = new PrefabDictionary();

        public void ClearDictionary()
        {
            prefabDictionary.Clear();
        }
        public void ClearDictionaryNullptr()
        {
            prefabDictionary.ClearNullptr();
        }
    }
}