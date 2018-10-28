using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LobbyMainPanelController : MonoBehaviour
{
    public Button StartAsServer;
    public Button StartAsClient;
    public GameObject ServerList;
    public NetworkLobbyManager LobbyManager;
    public NetworkDiscoveryManager Discovery;

    public void ServerStart()
    {
        Discovery.broadcastData = "DefaultRoom";
        Discovery.Initialize();
        Discovery.StartAsServer();
        NetworkManager.singleton.StartHost();
    }

    public void ClientStart()
    {
        Discovery.Initialize();
        Discovery.StartAsClient();
        ServerList.SetActive(true);
        StartAsServer.gameObject.SetActive(false);
        StartAsClient.gameObject.SetActive(false);
        this.gameObject.SetActive(false);
    }
}
