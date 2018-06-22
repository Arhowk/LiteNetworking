using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LobbyHostIntroductionPacket : LiteNetworking.LitePacket
{
    public int myPlayerId;
    public int[] activePlayerIds;

    public long[] preplacedInstanceIds;
    public int[] instanceIdToEntity;

    public override void Execute()
    {
        Debug.Log("I recieved my introduction packet!");
        // Create the locak player
        LiteNetworking.LobbyConnector.CreatePlayer(true, myPlayerId);
        

        // Create other players
        foreach(int i in activePlayerIds)
        {
            Debug.Log("Creating na aux player for " + i);
            LiteNetworking.LobbyConnector.CreatePlayer(false, i);
        }

        NetworkEvents.onLobbyJoined?.Invoke();
    }
}

public class LobbyNewPlayerPacket : LiteNetworking.LitePacket
{
    public int newPlayerId;

    public override void Execute()
    {
        LiteNetworking.LobbyConnector.CreatePlayer(false, newPlayerId);
    }
}


public class LobbyGoodbyePacket : LiteNetworking.LitePacket
{
    public int playerId;

    public override void Execute()
    {
        LiteNetworking.LobbyConnector.OnPlayerDisconnect(playerId);
    }
}


public class LobbyPackets : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
