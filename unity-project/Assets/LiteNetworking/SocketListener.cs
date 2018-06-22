using LiteNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SocketListener
{


    public static void ProcessRecieve()
    {
        while (true)
        {
            int recHostId;
            int connectionId;
            int channelId;
            byte[] recBuffer = new byte[1024];
            int bufferSize = 1024;
            int dataSize;
            byte error;
            NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
            //if (recData != NetworkEventType.Nothing) Debug.Log("Recieve : " + recData);
            switch (recData)
            {
                case NetworkEventType.Nothing: return;
                case NetworkEventType.ConnectEvent:
                    {

                        if (LobbyConnector.connectionId == connectionId)
                        {
                            //my connect request was approved
                            Debug.Log("On connect socces!!!");
                            LobbyConnector.OnConnectSuccess();
                        }
                        else
                        {
                            // another user connected
                            Debug.Log("On Player Joined");
                            LobbyConnector.OnPlayerJoined(connectionId);
                        }
                        break;
                    };
                case NetworkEventType.DataEvent:
                    {
                        //DispersePacket(new MemoryStream(recBuffer));3
                        if(LobbyConnector.isServer)
                        {
                            foreach(int connection in LobbyConnector.connectedClients)
                            {
                                if(connection != connectionId)
                                {
                                    SocketSender.SendPacket(new System.IO.MemoryStream(recBuffer, 0, recBuffer.Length, false, true), connection);
                                }
                            }
                            LitePacket.executingClient = LobbyConnector.connectionToPlayer[connectionId];
                            Networking.localPacketPlayer = Networking.GetPlayer(LitePacket.executingClient);
                        }
                        LiteNetworkingGenerated.PacketReader.ReadPacket(new System.IO.MemoryStream(recBuffer, 0, recBuffer.Length, false, true));
                        break;
                    };
                case NetworkEventType.DisconnectEvent:
                    {
                        if (LobbyConnector.connectionId == connectionId)
                        {
                            // cant connect to server
                        }
                        else
                        {
                            // someone disconnected
                            LobbyConnector.OnPlayerDisconnect(connectionId);
                        }
                        break;
                    }
                case NetworkEventType.BroadcastEvent:

                    break;
            }
        }
    }
}