using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地图系统的组件，用于记录Group数据
/// </summary>
[ExecuteInEditMode]
public class MapSystemComponent : MonoBehaviour
{

    private void OnEnable()
    {
        GameSystem.MapSystem.mapSystemComponent = this;

    }
}
