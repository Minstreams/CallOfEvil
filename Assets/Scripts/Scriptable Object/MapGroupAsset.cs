using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 单独的场景组，存到资源中
/// </summary>
public class MapGroupAsset : ScriptableObject
{
    /// <summary>
    /// 组名和场景名一样
    /// </summary>
    public string groupName;

    public GameObject groupPrefab;
}
