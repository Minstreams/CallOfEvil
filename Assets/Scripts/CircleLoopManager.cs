using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 鬼打墙地图管理器
/// </summary>
public class CircleLoopManager : MonoBehaviour
{
    private static CircleLoopManager instance;
    private static CircleLoopManager Instance
    {
        get
        {
#if UNITY_EDITOR
            if (instance == null)
            {
                Debug.LogError("No Circle Loop Manager Found!");
                Debug.Break();
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
    public float currentAngle;

    /// <summary>
    /// 最大圈数(n)
    /// </summary>
    public int maxCircleCount;

    //Unit控制-------------------------------------------------------------------
    public LinkedList<CLUnit> unitList = new LinkedList<CLUnit>();

    /// <summary>
    /// 最小值边界节点（范围外）
    /// </summary>
    private LinkedListNode<CLUnit> minBorderNode;
    /// <summary>
    /// 最大值边界节点（范围外）
    /// </summary>
    private LinkedListNode<CLUnit> maxBorderNode;

    /// <summary>
    /// 环向搜索的下一个
    /// </summary>
    private LinkedListNode<CLUnit> GetNext(LinkedListNode<CLUnit> unit)
    {
        if (unit.Next != null) return unit.Next;
        else return unit.List.First;
    }
    /// <summary>
    /// 环向搜索的上一个
    /// </summary>
    private LinkedListNode<CLUnit> GetPrevious(LinkedListNode<CLUnit> unit)
    {
        if (unit.Previous != null) return unit.Previous;
        else return unit.List.Last;
    }

    public void AddUnit(CLUnit unit)
    {
        //计算Angle
        float angle = unit.angle = GetUnitAngle(unit.transform.position);

        if (unitList.Count == 0)
        {
            minBorderNode = maxBorderNode = unitList.AddFirst(unit);
        }
        else
        {
            //简单插入排序
            LinkedListNode<CLUnit> u = unitList.First;
            while (u != null && u.Value.angle < angle) u = u.Next;
            if (u == null) unitList.AddLast(unit);
            else unitList.AddBefore(u, unit);
        }
    }


    /// <summary>
    /// 根据位置获取Unit角度，用于排序
    /// </summary>
    /// <param name="unitPos">位置</param>
    /// <returns>角度</returns>
    public static float GetUnitAngle(Vector3 unitPos)
    {
        return CurrentCircle * 360 + Vector2.Angle(Vector2.right, unitPos);
    }
}
