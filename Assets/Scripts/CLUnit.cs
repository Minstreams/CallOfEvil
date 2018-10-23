using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地图物体单位
/// </summary>
[DisallowMultipleComponent]
public class CLUnit : MonoBehaviour
{
    /// <summary>
    /// 在CLManager注册的角度，用于排序和动态加载
    /// </summary>
    public float angle;
    /// <summary>
    /// 在CLManager注册的index，用于网络同步,只需要在Group保存时，和生成最终地图时更新
    /// </summary>
    public int index;
    /// <summary>
    /// unit激活状态
    /// </summary>
    public bool Active { get; private set; }

    public void SetActive(bool b)
    {
        //if (Active == b) return;
        Active = b;
        gameObject.SetActive(b);
    }

    //TODO：
    //激活
    //合并打包（所有无功能静态Unit打包到一块）
    //依据恐慌值变形


#if UNITY_EDITOR
    private void OnDestroy()
    {
        //gameObject.SetActive(true);
        CircleLoopManager.DeleteUnit(this);
    }

#endif
}
