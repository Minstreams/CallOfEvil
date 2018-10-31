using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using GameSystem;

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

    public static MapManagerStyle Style { get { return EditorSystem.MapManager.Style; } }

    private const float bezierFactor = 0.5522847498307933984022516322796f * MapSystem.AnglePerGroup / 90; //贝塞尔系数
    private static readonly float bezierRadius = Mathf.Sqrt(bezierFactor * bezierFactor + 1);   //贝塞尔半径
    private static readonly float bezierAngle = Mathf.Atan(bezierFactor);  //贝塞尔角
    private const float angleToPiRate = Mathf.PI / 180f;

    private static AnimFloat outline0 = new AnimFloat(0);
    private static AnimFloat outline1 = new AnimFloat(0);
    private static AnimFloat outline2 = new AnimFloat(0);

    [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
    static void OnGizmo(MapSystemComponent manager, GizmoType type)
    {
        float handleSize = HandleUtility.GetHandleSize(Vector3.zero);
        Color gradient = Style.circleGradient.Evaluate(MapSystem.currentAngle / MapSystem.MaxAngle);

        ////获取注视点并更新
        //Vector3 hitPos;
        //if (focusMouse && !EditorApplication.isPlaying)
        //{
        //    Ray mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        //    float ty = mouseRay.origin.y / mouseRay.direction.y;
        //    hitPos = new Vector3(mouseRay.origin.x - mouseRay.direction.x * ty, 0, mouseRay.origin.z - mouseRay.direction.z * ty);
        //}
        //else hitPos = Camera.current.transform.position;

        ////if (hitPos.magnitude > Style.r0)
        //MapSystem.SetCurrentAngle(MapSystem.GetAngle(hitPos));
        ////Handles.SphereHandleCap(0, hitPos, Quaternion.identity, 0.5f, EventType.Repaint);




        //绘制低半部分指示圆环
        float alpha = MapSystem.currentAngle % MapSystem.AnglePerGroup;
        float beta = MapSystem.AnglePerGroup / 2 - alpha;

        float pAngle0 = MapSystem.currentGroupIndex % MapSystem.GroupCountPerCircle * MapSystem.AnglePerGroup * angleToPiRate;
        Vector3 p0 = new Vector3(Mathf.Cos(pAngle0) * Style.r0, -Style.circleVerticalCurve.Evaluate(Mathf.Abs(beta)) * Style.circleVerticleRate * alpha * angleToPiRate, Mathf.Sin(pAngle0) * Style.r0);

        float alphai = alpha + MapSystem.AnglePerGroup;
        float phi = (MapSystem.AnglePerGroup / 2 + alpha);
        float pAngleii = pAngle0;
        float pAnglei = pAngle0 - MapSystem.AnglePerGroup * angleToPiRate;
        Vector3 ppi = p0, ppit;
        Vector3 pi, pit;
        while (alphai <= 180)
        {
            ppit = new Vector3(Mathf.Cos(pAngleii - bezierAngle) * Style.r0 * bezierRadius, ppi.y - Style.circleVerticalCurve.Evaluate(phi) * Style.circleVerticleRate * bezierAngle, Mathf.Sin(pAngleii - bezierAngle) * Style.r0 * bezierRadius);
            pi = new Vector3(Mathf.Cos(pAnglei) * Style.r0, ppi.y - Style.circleVerticalCurve.Evaluate(phi) * Style.circleVerticleRate * MapSystem.AnglePerGroup * angleToPiRate, Mathf.Sin(pAnglei) * Style.r0);
            pit = new Vector3(Mathf.Cos(pAnglei + bezierAngle) * Style.r0 * bezierRadius, pi.y + Style.circleVerticalCurve.Evaluate(phi) * Style.circleVerticleRate * bezierAngle, Mathf.Sin(pAnglei + bezierAngle) * Style.r0 * bezierRadius);


            Handles.DrawBezier(ppi, pi, ppit, pit, Color.Lerp(gradient, Style.backColor, Style.circleColorCurve.Evaluate(phi)), null, Style.circleWidth);
            Handles.DrawBezier(ppi * Style.r2, pi * Style.r2, ppit, pit, Color.Lerp(gradient, Style.backColor, Style.circleColorCurve.Evaluate(phi)), null, Style.circleWidth);
            Handles.DrawBezier(ppi * Style.r3, pi * Style.r3, ppit, pit, Color.Lerp(gradient, Style.backColor, Style.circleColorCurve.Evaluate(phi)), null, Style.circleWidth);

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

        ppit = new Vector3(Mathf.Cos(pAngleii - newBezierAngle) * Style.r0 * newBezierRadius, ppi.y - Style.circleVerticalCurve.Evaluate(phi) * Style.circleVerticleRate * newBezierAngle, Mathf.Sin(pAngleii - newBezierAngle) * Style.r0 * newBezierRadius);
        pi = new Vector3(Mathf.Cos(pAnglei) * Style.r0, ppi.y - Style.circleVerticalCurve.Evaluate(phi) * Style.circleVerticleRate * newAngle * angleToPiRate, Mathf.Sin(pAnglei) * Style.r0);
        pit = new Vector3(Mathf.Cos(pAnglei + newBezierAngle) * Style.r0 * newBezierRadius, pi.y + Style.circleVerticalCurve.Evaluate(phi) * Style.circleVerticleRate * newBezierAngle, Mathf.Sin(pAnglei + newBezierAngle) * Style.r0 * newBezierRadius);

        Handles.DrawBezier(ppi, pi, ppit, pit, Color.Lerp(gradient, Style.backColor, Style.circleColorCurve.Evaluate(phi)), null, Style.circleWidth);
        Handles.DrawBezier(ppi * Style.r2, pi * Style.r2, ppit * Mathf.Lerp(Style.r2, 1, newAngle / MapSystem.AnglePerGroup), pit * Mathf.Lerp(Style.r2, 1, newAngle / MapSystem.AnglePerGroup), Color.Lerp(gradient, Style.backColor, Style.circleColorCurve.Evaluate(phi)), null, Style.circleWidth);
        Handles.DrawBezier(ppi * Style.r3, pi * Style.r3, ppit * Mathf.Lerp(Style.r3, 1, newAngle / MapSystem.AnglePerGroup), pit * Mathf.Lerp(Style.r3, 1, newAngle / MapSystem.AnglePerGroup), Color.Lerp(Style.circleGradient.Evaluate(MapSystem.currentAngle / MapSystem.MaxAngle), Style.backColor, Style.circleColorCurve.Evaluate(phi)), null, Style.circleWidth);





        //绘制可见范围
        Handles.color = Style.arcColor;
        Handles.DrawSolidArc(Vector3.zero, Vector3.up, Quaternion.Euler(-Vector3.up * (MapSystem.currentAngle - MapSystem.AnglePerGroup)) * Vector3.right, 360 - 2 * MapSystem.AnglePerGroup, Style.arcRadius * handleSize);
        Handles.color = Color.white;




        //绘制高半部分指示圆环
        alphai = MapSystem.AnglePerGroup - alpha;
        phi = beta;
        pAngleii = pAngle0;
        pAnglei = pAngle0 + MapSystem.AnglePerGroup * angleToPiRate;

        ppi = p0;
        while (alphai <= 180)
        {
            ppit = new Vector3(Mathf.Cos(pAngleii + bezierAngle) * Style.r0 * bezierRadius, ppi.y + Style.circleVerticalCurve.Evaluate(Mathf.Abs(phi)) * Style.circleVerticleRate * bezierAngle, Mathf.Sin(pAngleii + bezierAngle) * Style.r0 * bezierRadius);
            pi = new Vector3(Mathf.Cos(pAnglei) * Style.r0, ppi.y + Style.circleVerticalCurve.Evaluate(Mathf.Abs(phi)) * Style.circleVerticleRate * MapSystem.AnglePerGroup * angleToPiRate, Mathf.Sin(pAnglei) * Style.r0);
            pit = new Vector3(Mathf.Cos(pAnglei - bezierAngle) * Style.r0 * bezierRadius, pi.y - Style.circleVerticalCurve.Evaluate(Mathf.Abs(phi)) * Style.circleVerticleRate * bezierAngle, Mathf.Sin(pAnglei - bezierAngle) * Style.r0 * bezierRadius);

            Handles.DrawBezier(ppi, pi, ppit, pit, Color.Lerp(gradient, Style.backColor, Style.circleColorCurve.Evaluate(Mathf.Abs(phi))), null, Style.circleWidth);
            Handles.DrawBezier(ppi * Style.r2, pi * Style.r2, ppit, pit, Color.Lerp(gradient, Style.backColor, Style.circleColorCurve.Evaluate(Mathf.Abs(phi))), null, Style.circleWidth);
            Handles.DrawBezier(ppi * Style.r3, pi * Style.r3, ppit, pit, Color.Lerp(gradient, Style.backColor, Style.circleColorCurve.Evaluate(Mathf.Abs(phi))), null, Style.circleWidth);

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

        ppit = new Vector3(Mathf.Cos(pAngleii + newBezierAngle) * Style.r0 * newBezierRadius, ppi.y + Style.circleVerticalCurve.Evaluate(Mathf.Abs(phi)) * Style.circleVerticleRate * newBezierAngle, Mathf.Sin(pAngleii + newBezierAngle) * Style.r0 * newBezierRadius);
        pi = new Vector3(Mathf.Cos(pAnglei) * Style.r0, ppi.y + Style.circleVerticalCurve.Evaluate(Mathf.Abs(phi)) * Style.circleVerticleRate * newAngle * angleToPiRate, Mathf.Sin(pAnglei) * Style.r0);
        pit = new Vector3(Mathf.Cos(pAnglei - newBezierAngle) * Style.r0 * newBezierRadius, pi.y - Style.circleVerticalCurve.Evaluate(Mathf.Abs(phi)) * Style.circleVerticleRate * newBezierAngle, Mathf.Sin(pAnglei - newBezierAngle) * Style.r0 * newBezierRadius);

        Handles.DrawBezier(ppi, pi, ppit, pit, Color.Lerp(gradient, Style.backColor, Style.circleColorCurve.Evaluate(phi)), null, Style.circleWidth);
        Handles.DrawBezier(ppi * Style.r2, pi * Style.r2, ppit * Mathf.Lerp(Style.r2, 1, newAngle / MapSystem.AnglePerGroup), pit * Mathf.Lerp(Style.r2, 1, newAngle / MapSystem.AnglePerGroup), Color.Lerp(gradient, Style.backColor, Style.circleColorCurve.Evaluate(phi)), null, Style.circleWidth);
        Handles.DrawBezier(ppi * Style.r3, pi * Style.r3, ppit * Mathf.Lerp(Style.r3, 1, newAngle / MapSystem.AnglePerGroup), pit * Mathf.Lerp(Style.r3, 1, newAngle / MapSystem.AnglePerGroup), Color.Lerp(gradient, Style.backColor, Style.circleColorCurve.Evaluate(phi)), null, Style.circleWidth);


        //绘制组边线(只对应每层三组的情况)
        int t = MapSystem.currentGroupIndex % 3;
        outline0.target = t != 1 ? 1 : 0;
        outline1.target = t != 2 ? 1 : 0;
        outline2.target = t != 0 ? 1 : 0;

        Quaternion yRot = Quaternion.Euler(0,-120, 0);
        Vector3 axis = Vector3.right;

        Handles.color = Color.Lerp(Color.clear, gradient, outline0.value);
        Handles.DrawAAPolyLine(Style.outlineWidth, axis * Style.r0 * Style.r3 * Style.outlineBegin, axis * Style.r0 * Style.r3 * Style.outlineEnd * handleSize);

        axis = yRot * axis;
        Handles.color = Color.Lerp(Color.clear, gradient, outline1.value);
        Handles.DrawAAPolyLine(Style.outlineWidth, axis * Style.r0 * Style.r3 * Style.outlineBegin, axis * Style.r0 * Style.r3 * Style.outlineEnd * handleSize);

        axis = yRot * axis;
        Handles.color = Color.Lerp(Color.clear, gradient, outline2.value);
        Handles.DrawAAPolyLine(Style.outlineWidth, axis * Style.r0 * Style.r3 * Style.outlineBegin, axis * Style.r0 * Style.r3 * Style.outlineEnd * handleSize);

        Handles.color = Color.white;


        //绘制中央信息
        Handles.Label(Vector3.zero, MapSystem.CurrentCircle.ToString(), Style.circleIndexStyle);
        Handles.Label(Vector3.zero * 1f, MapSystem.currentAngle.ToString(), Style.angleStyle);



        //绘制小人

        float rr2 = Style.r0 * Mathf.Lerp(1, Style.r2, Mathf.Abs(beta) * 2f / MapSystem.AnglePerGroup);

        Handles.color = Style.arrowColor;
        Handles.ArrowHandleCap(0, new Vector3(Mathf.Cos(MapSystem.currentAngle * angleToPiRate), 0, Mathf.Sin(MapSystem.currentAngle * angleToPiRate)) * rr2, Quaternion.Euler(-90, 0, 0), Style.arrowSize, EventType.Repaint);
        Handles.SphereHandleCap(0, new Vector3(Mathf.Cos(MapSystem.currentAngle * angleToPiRate) * rr2, Style.headHeight * Style.arrowSize, Mathf.Sin(MapSystem.currentAngle * angleToPiRate) * rr2), Quaternion.identity, Style.headSize * Style.arrowSize, EventType.Repaint);
        Handles.color = Color.white;
    }
}
