using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkLobby : NetworkLobbyManager
{

    public List<Transform> SpawnPoints;
    public GameObject ServerList;

    void Start()
    {
        ServerList = GameObject.FindGameObjectWithTag("ServerList");
    }

    public override void OnLobbyClientConnect(NetworkConnection conn)
    {
        base.OnLobbyClientConnect(conn);
        Debug.Log("Lobby Client Connected! " + "[id: " + conn.connectionId + "] [address: " + conn.address + "]");

    }
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        Debug.Log("Client Connected! " + "[id: " + conn.connectionId + "] [address: " + conn.address + "]");
    }

    //public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
    //{
    //    Debug.Log("Player Creating! " + "[id: " + conn.connectionId + "] [address: " + conn.address + "]");
    //    GameObject obj = Instantiate(lobbyPlayerPrefab.gameObject) as GameObject;
    //    Debug.Log("Player Created! " + "[id: " + conn.connectionId + "] [address: " + conn.address + "]");
    //    return obj;
    //}
}
