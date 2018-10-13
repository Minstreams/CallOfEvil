using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerListController : MonoBehaviour {
    public GameObject ServerInfo;
    public GameObject Content;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void AddServerInfo(string address, string name)
    {
        GameObject info = GameObject.Instantiate(ServerInfo, Content.transform);
        info.GetComponent<RoomInfo>().ServerAddress = address;
        info.GetComponent<RoomInfo>().RoomName = name;
    }
}
