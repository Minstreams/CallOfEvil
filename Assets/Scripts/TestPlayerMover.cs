using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TestPlayerMover : NetworkBehaviour {
    private float movement;
    public Rigidbody m_Rigidbody;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (!isLocalPlayer)
            return;
        movement = Input.GetAxis("Horizontal");
        Vector3 move = new Vector3(1f,0f,0f) * movement  * 10 * Time.deltaTime;
        m_Rigidbody.MovePosition(m_Rigidbody.position + move);
    }
}
