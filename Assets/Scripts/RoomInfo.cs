using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class RoomInfo : MonoBehaviour {
    public GameObject Manager;
    public GameObject ServerList;
    public NetworkDiscoveryManager discovery;
    public string ServerAddress;
    public string RoomName;
    public Text Address;
    public Text Name;
	// Use this for initialization
	void Start () {
        ServerList = GameObject.FindGameObjectWithTag("ServerList");
        GetComponent<UIButton>().OnDoubleClick.AddListener(Clicked);
        Manager = GameObject.FindGameObjectWithTag("LobbyManager");
	}
	
	// Update is called once per frame
	void Update () {
        Address.text = ServerAddress;
        Name.text = RoomName;
	}

    void Clicked()
    {
        
        discovery = GameObject.FindGameObjectWithTag("discovery").GetComponent<NetworkDiscoveryManager>();
        discovery.StopBroadcast();
        NetworkLobby lobby = Manager.GetComponent<NetworkLobby>();
        lobby.networkAddress = ServerAddress;
        lobby.StartClient();
        ServerList.SetActive(false);
    }
}
