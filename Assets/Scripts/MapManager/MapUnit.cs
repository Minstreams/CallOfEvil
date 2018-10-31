using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地图物体单位
/// </summary>
[DisallowMultipleComponent]
[ExecuteInEditMode]
public class MapUnit : MonoBehaviour
{
    /// <summary>
    /// 在MapSystem注册的角度，用于排序和动态加载
    /// </summary>
    public float angle;
    /// <summary>
    /// 在MapGroup注册的index，用于网络同步
    /// </summary>
    public int index;


    //TODO：
    //合并打包（所有无功能静态Unit打包到一块）
    //依据恐慌值变形

}
