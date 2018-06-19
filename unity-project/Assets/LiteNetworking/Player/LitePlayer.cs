using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetworking;

public partial class LitePlayer : NetworkedEntity {
    public int id;

    public void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public bool IsBot()
    {
        return false;
    }

    public bool IsRealPlayer()
    {
        return true;
    }

    public int GetConnectionId()
    {
        /*if(!Networking.isServer)
        {
            
        }*/
        return LobbyConnector.ConvertPlayerToConnection(this);
    }
}
