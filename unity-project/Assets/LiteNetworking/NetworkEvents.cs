using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkEvents {
    public delegate void OnMatchmakerConnected();
    public static OnMatchmakerConnected onMatchmakerConnected;

    public delegate void OnMatchmakerRecievedLobbies();
    public static OnMatchmakerRecievedLobbies onMatchmakerRecievedLobbies;
   
    public delegate void OnHostStarted();
    public static OnHostStarted onHostStarted;

    public delegate void OnPlayerConnected();
    public static OnPlayerConnected onPlayerConnected;

    public delegate void OnLobbyJoined();
    public static OnLobbyJoined onLobbyJoined;

    public delegate void OnPlayerDisconnect();
    public static OnPlayerDisconnect onPlayerDisconnect;

    public delegate void OnDisconnect();
    public static OnDisconnect onDisconnect;

}
