using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class RoomInfo : MonoBehaviour {
    public GameObject Manager;
    public string ServerAddress;
    public string RoomName;
    public Text Address;
    public Text Name;
	// Use this for initialization
	void Start () {
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
        NetworkLobbyManager lobby = Manager.GetComponent<NetworkLobbyManager>();
        lobby.networkAddress = ServerAddress;
        lobby.StartClient();
    }
}
