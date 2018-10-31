using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地图系统的组件，用于记录Group数据
/// </summary>
[ExecuteInEditMode]
public class MapSystemComponent : MonoBehaviour {
    /// <summary>
    /// 组记录表
    /// </summary>
    public List<MapGroup> groupList = new List<MapGroup>();
    /// <summary>
    /// 当前最大圈数,游戏流程推进的时候，更改这个值并重新生成地图
    /// </summary>
    public int circleCount;

    private void Awake()
    {
        GameSystem.MapSystem.mapSystemComponent = this;
    }

}
