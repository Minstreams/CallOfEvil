using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem;

/// <summary>
/// 母体代理，用于给UnityEvents传递系统方法
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("自制工具/母体代理")]
public class TheMatrixAgent : MonoBehaviour {
    [Header("母体代理，用于给UnityEvents传递系统方法")]
    public GameMessage messageToSend;

    public void SendGameMessage()
    {
        TheMatrix.SendGameMessage(messageToSend);
    }
}
