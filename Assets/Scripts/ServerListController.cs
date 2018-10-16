using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerListController : MonoBehaviour {
    public GameObject ServerInfo;
    public GameObject Content;
    Dictionary<string, string> Infos;
    List<GameObject> ServerInfos;
	// Use this for initialization
	void Start () {
        Infos = new Dictionary<string, string>();
        ServerInfos = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void AddServerInfo(string address, string name)
    {
        if (Infos.ContainsKey(address)) return;
        else
        {
            Infos.Add(address, name);
            GameObject info = GameObject.Instantiate(ServerInfo, Content.transform);
            ServerInfos.Add(info);
            info.GetComponent<RoomInfo>().ServerAddress = address;
            info.GetComponent<RoomInfo>().RoomName = name;
        }

    }

    /// <summary>
    /// 刷新服务器列表
    /// </summary>
    public void Refresh()
    {
        Infos.Clear();
        foreach(GameObject i in ServerInfos)
        {
            Destroy(i);
        }
        ServerInfos.Clear();
    }

}
