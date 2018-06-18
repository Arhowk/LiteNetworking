using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetworking;

public class DebugLobbyConnector : MonoBehaviour {

    private string textFieldString = "localhost:8080";
    bool isConnected = false;
    bool isConnecting = false;
    // Use this for initialization
    void Start ()
    {
        print("Setting ttrget fps");
        if(UnityEngine.SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 30;
        }
        NetworkingConfig cfg = new NetworkingConfig();

        cfg.isServer = false;
        cfg.isMatchmaker = false;
        cfg.simulatedLatency = 100;

        LobbyConnector.Init(cfg);

        NetworkEvents.onLobbyJoined += OnLobbyJoined;
        NetworkEvents.onDisconnect += OnLobbyDisconnected;
        NetworkEvents.onPlayerConnected += OnPlayerConnected;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnGUI()
    {
        if(isConnecting)
        {
            if(GUILayout.Button("Cancel Connection"))
            {
                this.isConnected = false;
                this.isConnecting = false;

                LobbyConnector.CancelConnection();
            }
        }else if(isConnected)
        {
            if(GUILayout.Button("Disconnect"))
            {
                LobbyConnector.Disconnect();
            }
        }
        else
        {
            if (GUILayout.Button("Start Host"))
            {
                this.isConnecting = true;

                LobbyInfo thisLobby = new LobbyInfo();
                thisLobby.ip = "localhost";
                thisLobby.port = 8080;
                thisLobby.isServer = false;
                LobbyConnector.HostLobby(thisLobby);
            }

            if (GUILayout.Button("Start Dedicated Server"))
            {
                this.isConnecting = true;

                LobbyInfo thisLobby = new LobbyInfo();
                thisLobby.ip = "localhost";
                thisLobby.port = 8080;
                thisLobby.isServer = false;
                LobbyConnector.HostLobby(thisLobby);
            }

            if (GUILayout.Button("Connect To Host"))
            {
                this.isConnecting = true;

                LobbyConnector.ConnectToLobby(textFieldString);
            }
            textFieldString = GUI.TextField(new Rect(0, 100, 100, 20), textFieldString);
        }
        
    }

    private void OnLobbyJoined()
    {
        isConnected = true;
        isConnecting = false;
    }

    private void OnPlayerConnected()
    {
        
    }

    private void OnLobbyDisconnected()
    {
        isConnecting = false;
        isConnected = false;
    }
}
