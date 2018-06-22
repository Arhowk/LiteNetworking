using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using LiteNetworking;

public class SocketSender : MonoBehaviour {
    public static void SendPacket(MemoryStream m, int connectionId = -1)
    {
        if (!Networking.isConnected) return;

        byte error;
        if(LobbyConnector.isServer)
        {
          //      Debug.Log("Send as host???");
            if(connectionId != -1)
            {
                SendToClient(m, connectionId);
            }
            else
            {
                foreach (int i in LobbyConnector.connectedClients)
                {
                    SendToClient(m, i);
                }
            }
        }
        else
        {
            SendToClient(m, LobbyConnector.connectionId);
        }
    }

    private static void SendToClient(MemoryStream m, int connectionId)
    {
        if (!Networking.isConnected) return;

        byte error;
        NetworkTransport.Send(
                   LobbyConnector.hostId,
                   connectionId,
                   LobbyConnector.reliableChannelId,
                   m.GetBuffer(),
                   (int)m.Length,
                   out error);


        if ((NetworkError)error != NetworkError.Ok)
        {
            //Output this message in the console with the Network Error
            //     Debug.Log("There was this error in sending the packet : " + (NetworkError)error);
        }
        else
        {
            //   print("Sent success");
        }
    }
}
