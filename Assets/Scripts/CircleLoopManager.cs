using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 鬼打墙地图管理器，记得Reset Transform
/// </summary>
public class CircleLoopManager : MonoBehaviour
{
    private static CircleLoopManager instance;
    public static CircleLoopManager Instance
    {
        get
        {
#if UNITY_EDITOR
            if (instance == null)
            {
                GameObject g = GameObject.Find("CircleLoopManager");
                if (g != null)
                    instance = g.GetComponent<CircleLoopManager>();
                if (g == null || instance == null)
                {
                    Debug.LogError("No Circle Loop Manager Found!");
                    Debug.Break();
                }
            }
#endif
            return instance;
        }
    }

    /// <summary>
    /// 当前圈数(0~n-1)
    /// </summary>
    public static int CurrentCircle { get { return Instance.currentCircle; } }
    public int currentCircle;

    /// <summary>
    /// 最大圈数(n)
    /// </summary>
    public int maxCircleCount;

    /// <summary>
    /// 当前角度
    /// </summary>
    public float currentAngle;

    public float angleRadius = 90;//角度动态半径

#if UNITY_EDITOR
    public float editorAngleRadius = 150;
#endif

    public float AngleRadius
    {
        get
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
                return editorAngleRadius;
#endif
            return angleRadius;
        }
    }



    //Angle计算------------------------------------------------------------------
    private static float MaxAngle { get { return Instance.maxCircleCount * 360; } }

    /// <summary>
    /// 根据位置获取Unit角度，用于排序
    /// </summary>
    /// <param name="unitPos">位置</param>
    /// <returns>角度</returns>
    public static float GetAngle(Vector3 unitPos)
    {
        Vector2 pos = new Vector2(unitPos.x, unitPos.z);
        //边界值可能会有问题，如果出问题可以考虑用Group位置信息辅助计算
        return CurrentCircle * 360 + Vector2.Angle(Vector2.right, pos);
    }
    /// <summary>
    /// 角度相减，返回差值
    /// </summary>
    private static float SubSigned(float angle1, float angle2)
    {
        float result = angle1 - angle2;
        float mHalf = MaxAngle / 2;
        if (result > mHalf) return result - MaxAngle;
        if (result < -mHalf) return result + MaxAngle;
        return result;
    }
    /// <summary>
    /// 角度加增量，返回角度
    /// </summary>
    /// <param name="angle">角度</param>
    /// <param name="increment">增量(-360~360)</param>
    private static float Rotate(float angle, float increment)
    {
        if (increment > 0) return Add(angle, increment);
        else return Sub(angle, -increment);
    }
    /// <summary>
    /// 角度相减，返回角度
    /// </summary>
    private static float Sub(float angle1, float angle2)
    {
        float result = angle1 - angle2;
        if (result < 0) return result + MaxAngle;
        return result;
    }
    /// <summary>
    /// 角度相加，返回角度
    /// </summary>
    private static float Add(float angle1, float angle2)
    {
        float result = angle1 + angle2;
        float m = MaxAngle;
        if (result >= m) return result - m;
        return result;
    }

    //Unit控制-------------------------------------------------------------------
    /// <summary>
    /// 按angle排序的有序表
    /// </summary>
    public List<CLUnit> unitList;

    /// <summary>
    /// 最小值边界节点（范围外）
    /// </summary>
    private int minBorderPtr;
    /// <summary>
    /// 最大值边界节点（范围外）
    /// </summary>
    private int maxBorderPtr;

    /// <summary>
    /// 环向搜索的下一个位置
    /// </summary>
    private int GetNext(int ptr)
    {
        if (ptr == unitList.Count - 1) return 0;
        else return ptr + 1;
    }
    /// <summary>
    /// 环向搜索的上一个
    /// </summary>
    private int GetPrevious(int ptr)
    {
        if (ptr == 0) return unitList.Count - 1;
        else return ptr - 1;
    }

    public void SetCurrentAngle(float angle)
    {
        if (unitList.Count == 0)
        {
            currentAngle = angle;
            return;
        }
        if (SubSigned(angle, currentAngle) > 0)
        {
            //正向更新检测

            //先扩展max再收min？

            //扩展max
            if (!unitList[maxBorderPtr].Active && Sub(unitList[maxBorderPtr].angle, Sub(angle, AngleRadius)) < 2 * AngleRadius)
            {
                unitList[maxBorderPtr].SetActive(true);
                maxBorderPtr = GetNext(maxBorderPtr);
            }

            //收缩min
            int minPlusPtr = GetNext(minBorderPtr);
            if (unitList[minPlusPtr].Active && Sub(Add(angle, AngleRadius), unitList[minPlusPtr].angle) > 2 * AngleRadius)
            {
                unitList[minPlusPtr].SetActive(false);
                minBorderPtr = minPlusPtr;
            }
        }
        else
        {
            //反向更新检测
            //扩展min
            if (!unitList[minBorderPtr].Active && Sub(Add(angle, AngleRadius), unitList[minBorderPtr].angle) < 2 * AngleRadius)
            {
                unitList[minBorderPtr].SetActive(true);
                minBorderPtr = GetPrevious(minBorderPtr);
            }

            //收缩max
            int maxSubPtr = GetPrevious(maxBorderPtr);
            if (unitList[maxSubPtr].Active && Sub(unitList[maxSubPtr].angle, Sub(angle, AngleRadius)) > 2 * AngleRadius)
            {
                unitList[maxSubPtr].SetActive(false);
                maxBorderPtr = maxSubPtr;
            }
        }
        currentAngle = angle;
    }

    //Group加载------------------------------------------------------------------
    //TODO：Group Save & Load

    /// <summary>
    /// 初始化所有Unit的Active状态并初始化ptr
    /// </summary>
    public void InitUnitActiveState()
    {
        float minAngle = Sub(currentAngle, AngleRadius);
        float maxAngle = Add(currentAngle, AngleRadius);

        if (minAngle < maxAngle)
        {
            int ptr = 0;
            while (ptr < unitList.Count && unitList[ptr].angle < minAngle)
            {
                unitList[ptr].SetActive(false);
                ptr++;
            }
            minBorderPtr = GetPrevious(ptr);
            while (ptr < unitList.Count && unitList[ptr].angle < maxAngle)
            {
                unitList[ptr].SetActive(true);
                ptr++;
            }
            maxBorderPtr = GetNext(ptr - 1);
            while (ptr < unitList.Count)
            {
                unitList[ptr].SetActive(false);
                ptr++;
            }
        }
        else
        {
            int ptr = 0;
            while (ptr < unitList.Count && unitList[ptr].angle < maxAngle)
            {
                unitList[ptr].SetActive(true);
                ptr++;
            }
            maxBorderPtr = GetNext(ptr - 1);
            while (ptr < unitList.Count && unitList[ptr].angle < minAngle)
            {
                unitList[ptr].SetActive(false);
                ptr++;
            }
            minBorderPtr = GetPrevious(ptr);
            while (ptr < unitList.Count)
            {
                unitList[ptr].SetActive(true);
                ptr++;
            }
        }
    }


#if UNITY_EDITOR
    //编辑器控制-----------------------------------------------------------------
    /// <summary>
    /// 排序插入
    /// </summary>
    public void AddUnitSorted(CLUnit unit)
    {
        //计算Angle
        float angle = unit.angle = GetAngle(unit.transform.position);

        if (unitList.Count == 0)
        {
            minBorderPtr = maxBorderPtr = 0;
            unitList.Add(unit);
        }
        else
        {
            //简单插入排序
            int ptr = unitList[minBorderPtr].angle < angle ? minBorderPtr : 0;

            while (ptr != unitList.Count && unitList[ptr].angle < angle) ptr++;
            unitList.Insert(ptr, unit);

            //ptr重定向
            if (minBorderPtr >= ptr) minBorderPtr++;
            if (maxBorderPtr > ptr) maxBorderPtr++;
        }
    }

    /// <summary>
    /// 删除
    /// </summary>
    public void _DeleteUnit(CLUnit unit)
    {
        int index = unitList.IndexOf(unit);
        unitList.RemoveAt(index);

        if (minBorderPtr >= index) minBorderPtr = GetPrevious(minBorderPtr);
        if (maxBorderPtr > index) maxBorderPtr = GetPrevious(maxBorderPtr);

        DestroyImmediate(unit);
    }
    public static void DeleteUnit(CLUnit unit)
    {
        Instance._DeleteUnit(unit);
    }

    [ContextMenu("ClearList")]
    public void ClearList()
    {
        unitList.Clear();
    }

#endif
}
