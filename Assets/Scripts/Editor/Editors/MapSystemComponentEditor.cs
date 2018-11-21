using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using GameSystem;
using EditorSystem;

/// <summary>
/// 这个Editor利用Gizmo绘制操作UI
/// </summary>
[CustomEditor(typeof(MapSystemComponent))]
public class MapSystemComponentEditor : Editor
{
    /// <summary>
    /// 注视鼠标，否则注视摄像机
    /// </summary>
    public static bool focusMouse = true;

    private static EditorMatrixPrefs Prefs { get { return EditorMatrix.Prefs; } }

    private const float bezierFactor = 0.5522847498307933984022516322796f * MapSystem.AnglePerGroup / 90; //贝塞尔系数
    private static readonly float bezierRadius = Mathf.Sqrt(bezierFactor * bezierFactor + 1);   //贝塞尔半径
    private static readonly float bezierAngle = Mathf.Atan(bezierFactor);  //贝塞尔角
    private const float angleToPiRate = Mathf.PI / 180f;

    private static AnimFloat outline0 = new AnimFloat(0);
    private static AnimFloat outline1 = new AnimFloat(0);
    private static AnimFloat outline2 = new AnimFloat(0);

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (EditorApplication.isPlaying)
        {
            if (GUILayout.Button("Generate")) MapSystem.GenerateMap(MapSystem.ChooseMapGenerationInfo(0), 0);
            if (GUILayout.Button("Init")) MapSystem.InitGroupActiveState();
        }
    }


    [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
    static void OnGizmo(MapSystemComponent manager, GizmoType type)
    {
        float handleSize = HandleUtility.GetHandleSize(Vector3.zero);
        Color gradient = Prefs.circleGradient.Evaluate(MapSystem.currentAngle / MapSystem.MaxAngle);


        //绘制低半部分指示圆环
        float alpha = MapSystem.currentAngle % MapSystem.AnglePerGroup;
        float beta = MapSystem.AnglePerGroup / 2 - alpha;

        float pAngle0 = MapSystem.currentGroupIndex % MapSystem.GroupCountPerCircle * MapSystem.AnglePerGroup * angleToPiRate;
        Vector3 p0 = new Vector3(Mathf.Cos(pAngle0) * Prefs.r0, -Prefs.circleVerticalCurve.Evaluate(Mathf.Abs(beta)) * Prefs.circleVerticleRate * alpha * angleToPiRate, Mathf.Sin(pAngle0) * Prefs.r0);

        float alphai = alpha + MapSystem.AnglePerGroup;
        float phi = (MapSystem.AnglePerGroup / 2 + alpha);
        float pAngleii = pAngle0;
        float pAnglei = pAngle0 - MapSystem.AnglePerGroup * angleToPiRate;
        Vector3 ppi = p0, ppit;
        Vector3 pi, pit;
        while (alphai <= 180)
        {
            ppit = new Vector3(Mathf.Cos(pAngleii - bezierAngle) * Prefs.r0 * bezierRadius, ppi.y - Prefs.circleVerticalCurve.Evaluate(phi) * Prefs.circleVerticleRate * bezierAngle, Mathf.Sin(pAngleii - bezierAngle) * Prefs.r0 * bezierRadius);
            pi = new Vector3(Mathf.Cos(pAnglei) * Prefs.r0, ppi.y - Prefs.circleVerticalCurve.Evaluate(phi) * Prefs.circleVerticleRate * MapSystem.AnglePerGroup * angleToPiRate, Mathf.Sin(pAnglei) * Prefs.r0);
            pit = new Vector3(Mathf.Cos(pAnglei + bezierAngle) * Prefs.r0 * bezierRadius, pi.y + Prefs.circleVerticalCurve.Evaluate(phi) * Prefs.circleVerticleRate * bezierAngle, Mathf.Sin(pAnglei + bezierAngle) * Prefs.r0 * bezierRadius);


            Handles.DrawBezier(ppi, pi, ppit, pit, Color.Lerp(gradient, Prefs.backColor, Prefs.circleColorCurve.Evaluate(phi)), null, Prefs.circleWidth);
            Handles.DrawBezier(ppi * Prefs.r2, pi * Prefs.r2, ppit, pit, Color.Lerp(gradient, Prefs.backColor, Prefs.circleColorCurve.Evaluate(phi)), null, Prefs.circleWidth);
            Handles.DrawBezier(ppi * Prefs.r3, pi * Prefs.r3, ppit, pit, Color.Lerp(gradient, Prefs.backColor, Prefs.circleColorCurve.Evaluate(phi)), null, Prefs.circleWidth);

            alphai += MapSystem.AnglePerGroup;
            phi += MapSystem.AnglePerGroup;
            pAngleii = pAnglei;
            pAnglei -= MapSystem.AnglePerGroup * angleToPiRate;
            ppi = pi;
        }

        pAnglei = (alpha - 180 + MapSystem.currentGroupIndex * MapSystem.AnglePerGroup) * angleToPiRate;

        float newAngle = (180 - alpha) % MapSystem.AnglePerGroup;
        float newBezierFactor = bezierFactor * (newAngle) / MapSystem.AnglePerGroup;
        float newBezierRadius = Mathf.Sqrt(newBezierFactor * newBezierFactor + 1);
        float newBezierAngle = Mathf.Atan(newBezierFactor);

        ppit = new Vector3(Mathf.Cos(pAngleii - newBezierAngle) * Prefs.r0 * newBezierRadius, ppi.y - Prefs.circleVerticalCurve.Evaluate(phi) * Prefs.circleVerticleRate * newBezierAngle, Mathf.Sin(pAngleii - newBezierAngle) * Prefs.r0 * newBezierRadius);
        pi = new Vector3(Mathf.Cos(pAnglei) * Prefs.r0, ppi.y - Prefs.circleVerticalCurve.Evaluate(phi) * Prefs.circleVerticleRate * newAngle * angleToPiRate, Mathf.Sin(pAnglei) * Prefs.r0);
        pit = new Vector3(Mathf.Cos(pAnglei + newBezierAngle) * Prefs.r0 * newBezierRadius, pi.y + Prefs.circleVerticalCurve.Evaluate(phi) * Prefs.circleVerticleRate * newBezierAngle, Mathf.Sin(pAnglei + newBezierAngle) * Prefs.r0 * newBezierRadius);

        Handles.DrawBezier(ppi, pi, ppit, pit, Color.Lerp(gradient, Prefs.backColor, Prefs.circleColorCurve.Evaluate(phi)), null, Prefs.circleWidth);
        Handles.DrawBezier(ppi * Prefs.r2, pi * Prefs.r2, ppit * Mathf.Lerp(Prefs.r2, 1, newAngle / MapSystem.AnglePerGroup), pit * Mathf.Lerp(Prefs.r2, 1, newAngle / MapSystem.AnglePerGroup), Color.Lerp(gradient, Prefs.backColor, Prefs.circleColorCurve.Evaluate(phi)), null, Prefs.circleWidth);
        Handles.DrawBezier(ppi * Prefs.r3, pi * Prefs.r3, ppit * Mathf.Lerp(Prefs.r3, 1, newAngle / MapSystem.AnglePerGroup), pit * Mathf.Lerp(Prefs.r3, 1, newAngle / MapSystem.AnglePerGroup), Color.Lerp(Prefs.circleGradient.Evaluate(MapSystem.currentAngle / MapSystem.MaxAngle), Prefs.backColor, Prefs.circleColorCurve.Evaluate(phi)), null, Prefs.circleWidth);





        //绘制可见范围
        if (MapManager.Active)
        {
            Handles.color = Prefs.arcColor;
            Handles.DrawSolidArc(Vector3.zero, Vector3.up, Quaternion.Euler(-Vector3.up * (MapSystem.currentAngle - MapSystem.AnglePerGroup)) * Vector3.right, 360 - 2 * MapSystem.AnglePerGroup, Prefs.arcRadius * handleSize);
            Handles.color = Color.white;
        }



        //绘制高半部分指示圆环
        alphai = MapSystem.AnglePerGroup - alpha;
        phi = beta;
        pAngleii = pAngle0;
        pAnglei = pAngle0 + MapSystem.AnglePerGroup * angleToPiRate;

        ppi = p0;
        while (alphai <= 180)
        {
            ppit = new Vector3(Mathf.Cos(pAngleii + bezierAngle) * Prefs.r0 * bezierRadius, ppi.y + Prefs.circleVerticalCurve.Evaluate(Mathf.Abs(phi)) * Prefs.circleVerticleRate * bezierAngle, Mathf.Sin(pAngleii + bezierAngle) * Prefs.r0 * bezierRadius);
            pi = new Vector3(Mathf.Cos(pAnglei) * Prefs.r0, ppi.y + Prefs.circleVerticalCurve.Evaluate(Mathf.Abs(phi)) * Prefs.circleVerticleRate * MapSystem.AnglePerGroup * angleToPiRate, Mathf.Sin(pAnglei) * Prefs.r0);
            pit = new Vector3(Mathf.Cos(pAnglei - bezierAngle) * Prefs.r0 * bezierRadius, pi.y - Prefs.circleVerticalCurve.Evaluate(Mathf.Abs(phi)) * Prefs.circleVerticleRate * bezierAngle, Mathf.Sin(pAnglei - bezierAngle) * Prefs.r0 * bezierRadius);

            Handles.DrawBezier(ppi, pi, ppit, pit, Color.Lerp(gradient, Prefs.backColor, Prefs.circleColorCurve.Evaluate(Mathf.Abs(phi))), null, Prefs.circleWidth);
            Handles.DrawBezier(ppi * Prefs.r2, pi * Prefs.r2, ppit, pit, Color.Lerp(gradient, Prefs.backColor, Prefs.circleColorCurve.Evaluate(Mathf.Abs(phi))), null, Prefs.circleWidth);
            Handles.DrawBezier(ppi * Prefs.r3, pi * Prefs.r3, ppit, pit, Color.Lerp(gradient, Prefs.backColor, Prefs.circleColorCurve.Evaluate(Mathf.Abs(phi))), null, Prefs.circleWidth);

            alphai += MapSystem.AnglePerGroup;
            phi += MapSystem.AnglePerGroup;
            pAngleii = pAnglei;
            pAnglei += MapSystem.AnglePerGroup * angleToPiRate;
            ppi = pi;
        }

        pAnglei = (alpha + 180 + MapSystem.currentGroupIndex * MapSystem.AnglePerGroup) * angleToPiRate;

        newAngle = (180 + alpha) % MapSystem.AnglePerGroup;
        newBezierFactor = bezierFactor * (newAngle) / MapSystem.AnglePerGroup;
        newBezierRadius = Mathf.Sqrt(newBezierFactor * newBezierFactor + 1);
        newBezierAngle = Mathf.Atan(newBezierFactor);

        ppit = new Vector3(Mathf.Cos(pAngleii + newBezierAngle) * Prefs.r0 * newBezierRadius, ppi.y + Prefs.circleVerticalCurve.Evaluate(Mathf.Abs(phi)) * Prefs.circleVerticleRate * newBezierAngle, Mathf.Sin(pAngleii + newBezierAngle) * Prefs.r0 * newBezierRadius);
        pi = new Vector3(Mathf.Cos(pAnglei) * Prefs.r0, ppi.y + Prefs.circleVerticalCurve.Evaluate(Mathf.Abs(phi)) * Prefs.circleVerticleRate * newAngle * angleToPiRate, Mathf.Sin(pAnglei) * Prefs.r0);
        pit = new Vector3(Mathf.Cos(pAnglei - newBezierAngle) * Prefs.r0 * newBezierRadius, pi.y - Prefs.circleVerticalCurve.Evaluate(Mathf.Abs(phi)) * Prefs.circleVerticleRate * newBezierAngle, Mathf.Sin(pAnglei - newBezierAngle) * Prefs.r0 * newBezierRadius);

        Handles.DrawBezier(ppi, pi, ppit, pit, Color.Lerp(gradient, Prefs.backColor, Prefs.circleColorCurve.Evaluate(phi)), null, Prefs.circleWidth);
        Handles.DrawBezier(ppi * Prefs.r2, pi * Prefs.r2, ppit * Mathf.Lerp(Prefs.r2, 1, newAngle / MapSystem.AnglePerGroup), pit * Mathf.Lerp(Prefs.r2, 1, newAngle / MapSystem.AnglePerGroup), Color.Lerp(gradient, Prefs.backColor, Prefs.circleColorCurve.Evaluate(phi)), null, Prefs.circleWidth);
        Handles.DrawBezier(ppi * Prefs.r3, pi * Prefs.r3, ppit * Mathf.Lerp(Prefs.r3, 1, newAngle / MapSystem.AnglePerGroup), pit * Mathf.Lerp(Prefs.r3, 1, newAngle / MapSystem.AnglePerGroup), Color.Lerp(gradient, Prefs.backColor, Prefs.circleColorCurve.Evaluate(phi)), null, Prefs.circleWidth);


        //绘制组边线(只对应每层三组的情况)
        int t = MapSystem.currentGroupIndex % 3;
        outline0.target = t != 1 ? 1 : 0;
        outline1.target = t != 2 ? 1 : 0;
        outline2.target = t != 0 ? 1 : 0;

        Quaternion yRot = Quaternion.Euler(0, -120, 0);
        Vector3 axis = Vector3.right;

        Handles.color = Color.Lerp(Color.clear, gradient, outline0.value);
        Handles.DrawAAPolyLine(Prefs.outlineWidth, axis * Prefs.r0 * Prefs.r3 * Prefs.outlineBegin, axis * Prefs.r0 * Prefs.r3 * Prefs.outlineEnd * handleSize);

        axis = yRot * axis;
        Handles.color = Color.Lerp(Color.clear, gradient, outline1.value);
        Handles.DrawAAPolyLine(Prefs.outlineWidth, axis * Prefs.r0 * Prefs.r3 * Prefs.outlineBegin, axis * Prefs.r0 * Prefs.r3 * Prefs.outlineEnd * handleSize);

        axis = yRot * axis;
        Handles.color = Color.Lerp(Color.clear, gradient, outline2.value);
        Handles.DrawAAPolyLine(Prefs.outlineWidth, axis * Prefs.r0 * Prefs.r3 * Prefs.outlineBegin, axis * Prefs.r0 * Prefs.r3 * Prefs.outlineEnd * handleSize);

        Handles.color = Color.white;


        //绘制中央信息
        Handles.Label(Vector3.zero, MapSystem.CurrentCircle.ToString(), Prefs.circleIndexStyle);
        Handles.Label(Vector3.zero * 1f, MapSystem.currentAngle.ToString(), Prefs.angleStyle);



        //绘制小人

        float rr2 = Prefs.r0 * Mathf.Lerp(1, Prefs.r2, Mathf.Abs(beta) * 2f / MapSystem.AnglePerGroup);

        Handles.color = Prefs.arrowColor;
        Handles.ArrowHandleCap(0, new Vector3(Mathf.Cos(MapSystem.currentAngle * angleToPiRate), 0, Mathf.Sin(MapSystem.currentAngle * angleToPiRate)) * rr2, Quaternion.Euler(-90, 0, 0), Prefs.arrowSize, EventType.Repaint);
        Handles.SphereHandleCap(0, new Vector3(Mathf.Cos(MapSystem.currentAngle * angleToPiRate) * rr2, Prefs.headHeight * Prefs.arrowSize, Mathf.Sin(MapSystem.currentAngle * angleToPiRate) * rr2), Quaternion.identity, Prefs.headSize * Prefs.arrowSize, EventType.Repaint);
        Handles.color = Color.white;
    }
}
