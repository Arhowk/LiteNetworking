using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetworking;

public class MmoDuelTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
        /*if(Root.IsBot())
        {
            ConnectToHost();
        }
        else
        {
            OpenNewLobby();
        }*/

	}

    public void OpenNewLobby()
    {
        // Setup the networker
        LobbyInfo thisLobby = new LobbyInfo();

        thisLobby.connectedClients = 1;
        thisLobby.maxClients = 1000;
        thisLobby.port = 8081;
        thisLobby.name = "MMO Duel Simulation";

        LobbyConnector.HostLobby(thisLobby);
    }

    public void ConnectToHost()
    {
        LobbyConnector.ConnectToLobby("localhost:8081");
    }
}
