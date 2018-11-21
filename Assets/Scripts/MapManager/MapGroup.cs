using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单独的场景组，放在场景组中所有Unit的父物体上
/// </summary>
public class MapGroup : MonoBehaviour
{
    public const string defaultName = "默认场景组";
    public string groupName { get { return gameObject.name; } set { if (gameObject.name != value) { dirty = true; gameObject.name = value; } } }

    private void Reset()
    {
        groupName = defaultName;
    }

    /// <summary>
    /// 在MapSystem注册的index，用于网络同步
    /// </summary>
    public int index = -1;

    /// <summary>
    /// 按angle排序的有序表
    /// </summary>
    public List<MapUnit> unitList = new List<MapUnit>();
    public GameObject ground = null;

    public MapUnit this[int i] { get { return unitList[i]; } }

    [SerializeField, HideInInspector]
    private bool active;
    public bool Active
    {
        get { return active; }
        set
        {
            if (active == value) return;
            active = value;

            //TODO:激活时的动作
            gameObject.SetActive(value);
        }
    }

#if UNITY_EDITOR
    [System.NonSerialized]
    public bool dirty = false;
#endif
}
