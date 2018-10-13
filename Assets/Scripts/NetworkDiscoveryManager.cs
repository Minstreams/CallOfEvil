using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


/// <summary>
/// 局域网对战用
/// 用于广播服务器信息（服务器端）或者发现服务器信息（服务端）
/// </summary>
public class NetworkDiscoveryManager : NetworkDiscovery {
    public NetworkManager Manager;
    public ServerListController list;
    string RoomName;
    public override void OnReceivedBroadcast(string fromAddress, string data)
    {

    }
}
