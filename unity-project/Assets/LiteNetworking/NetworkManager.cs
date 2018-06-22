using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetworking;

public class NetworkManager : MonoBehaviour {
    public static NetworkManager inst;
    public bool useDebugUI = false;
    public GameObject playerPrefab;
    public GameObject[] temporaryEntityPrefabs;

	// Use this for initialization
	void Start () {
        if(inst)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            inst = this;
            if (useDebugUI)
            {
                gameObject.AddComponent<DebugLobbyConnector>();
            }

            foreach (GameObject g in temporaryEntityPrefabs)
            {
                print("Object : " + g);
                g.GetComponent<NetworkIdentity>().OnStartClient();
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
        SocketListener.ProcessRecieve();
    }
}
