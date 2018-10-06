using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem;

/// <summary>
/// 母体代理，用于给The Matrix发送控制信息。
/// 禁止在同一GameObject上存在多个，防止UnityEvents引用出错。
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("自制工具/母体代理")]
public class TheMatrixAgent : MonoBehaviour
{
    [Header("母体代理，用于给The Matrix发送控制信息")]
    public GameMessage messageToSend;

    public void SendGameMessage()
    {
        TheMatrix.SendGameMessage(messageToSend);
    }
}
