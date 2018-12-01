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

        Debug.Log("MapSystemComponent OnEnable.");
        if (!UnityEditor.EditorApplication.isPlaying)
            GameSystem.MapSystem.Init(this);
    }
#endif
    private void OnDisable()
    {
        Debug.Log("MapSystemComponent OnDisable.");
    }

    private void Awake()
    {
        Debug.Log("MapSystemComponent Awake.");
#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlaying)
#endif
            GameSystem.MapSystem.Init(this);
    }
}
