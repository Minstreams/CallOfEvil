using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkLobby : NetworkLobbyManager {

    public List<Transform> SpawnPoints;
    public GameObject ServerList;

    void Start()
    {
        ServerList = GameObject.FindGameObjectWithTag("ServerList");
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        Debug.Log("Client Connected!");
    }

    public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
    {

        GameObject obj = Instantiate(lobbyPlayerPrefab.gameObject) as GameObject;
        return obj;
    }
}
