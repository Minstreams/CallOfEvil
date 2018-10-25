using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Circle Loop Style", menuName = "系统配置文件/Circle Loop Style")]
public class CircleLoopManagerStyle : ScriptableObject
{
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
}
