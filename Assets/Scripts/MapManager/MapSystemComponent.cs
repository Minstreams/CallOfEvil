using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地图系统的组件，用于记录Group数据
/// </summary>
[ExecuteInEditMode]
public class MapSystemComponent : MonoBehaviour
{
#if UNITY_EDITOR
    private void OnEnable()
    {
        GameSystem.MapSystem.Init(this);
    }
#endif
    private void Awake()
    {
        GameSystem.MapSystem.Init(this);
    }
}
