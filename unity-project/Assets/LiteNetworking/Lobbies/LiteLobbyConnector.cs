using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LiteNetworkingGenerated;

namespace LiteNetworking
{

    /*
     * Defines the type of connection that you want the game to do
     */
    public enum NetworkingType
    {
        // All clients connect to one single host that manages all communications
        DEDICATED_SERVER,

        [Obsolete("Not implemented yet")]
        P2P_NO_HOST,

        [Obsolete("Not implemented yet")]
        P2P_WEAK_HOST,

        // One of the clients acts as a dedicated server as well as a player
        P2P_DEDICATED_HOST
    }
    public class NetworkingConfig
    {
        public bool isServer = false;
        public bool isMatchmaker = false;

        public NetworkingType type = NetworkingType.DEDICATED_SERVER;

        public string matchmakerServer = "localhost";
        public int matchmakerPort = 8081;

        public int simulatedLatency = 0;
    }

    public class LobbyInfo
    {
        public string name, host;
        public int connectedClients, maxClients, port;
        public bool isServer = false;
        public string ip;
    }

    public class LobbySetupInfo
    {


    }

    

    public class LobbyConnector
    {
        private static LobbyInfo currentLobby;
        public static int hostId, reliableChannelId, unreliableChannelId, connectedHost;
        public static bool isServer = false;
        public static bool isConnected = false;
        private static bool isInitialized = false;
        public static int connectionId;
        private static int nextPlayerIndex = 1;
        public static Dictionary<int, int> connectionToPlayer = new Dictionary<int, int>();

        public static List<int> connectedClients = new List<int>();

        static LobbyConnector()
        {
        }

        public static void Init(NetworkingConfig cfg)
        {
            Debug.Log("Init!");
            isInitialized = true;

            ConnectionConfig config = new ConnectionConfig();
            reliableChannelId = config.AddChannel(QosType.Reliable);
            unreliableChannelId = config.AddChannel(QosType.Unreliable);

            // An example of initializing the Transport Layer with custom settings
            // GlobalConfig gConfig = new GlobalConfig();
            //g//Config.MaxPacketSize = 9000;
            NetworkTransport.Init();



            if (isServer)
            {
               // int maxClients = 10;
               // HostTopology topology = new HostTopology(config, maxClients);
                //hostId = NetworkTransport.AddHost(topology, 8080);
            }
            else
            {

            }
        }

        

        public static void HostLobby(LobbyInfo i)
        {
            ConnectionConfig config = new ConnectionConfig();
            int myReliableChannelId = config.AddChannel(QosType.Reliable);
            int myUnreliableChannelId = config.AddChannel(QosType.Unreliable);
            HostTopology topology = new HostTopology(config, 10);
            hostId = NetworkTransport.AddHost(topology, 8080);
            isServer = true;
            currentLobby = i;
            if (!i.isServer)
            {
                // Connect the host
                connectedClients.Add(0);
                connectionToPlayer[0] = 0;

                SpawnPlayerPrefab(true);
            }

            MonoBehaviour.Instantiate(Resources.Load("Server Entity", typeof(GameObject)) as GameObject);
            isConnected = true;
            EntityManager.StartServer();
        }

        public static LitePlayer ConvertConnectionToPlayer(int connectionId)
        {
            if (connectionId == -1) return null;

            if (connectionToPlayer.ContainsKey(connectionId))
            {

                return Networking.GetPlayer(connectionToPlayer[connectionId]);
            }
            else
            {
                if (Networking.players.Count == 0) return null;
                return Networking.players[Networking.players.Keys.GetEnumerator().Current];
            }
        }

        public static int ConvertPlayerToConnection(LitePlayer p)
        {
            int id = p.id;


            foreach (KeyValuePair<int, int> pID in connectionToPlayer)
            {
                if (id == pID.Value)
                {
                    return pID.Key;
                }
            }

            return -1;
        }

        public static float GetPing(int playerId)
        {
            foreach(KeyValuePair<int,int> pID in connectionToPlayer)
            {
                if(playerId == pID.Value)
                {
                    byte err;
                    return NetworkTransport.GetCurrentRTT(hostId, pID.Key, out err);
                }
            }
            return -1;
        }

        public static void OnConnectSuccess()
        {
            // Spawn u
            //SpawnPlayerPrefab(true);

            // Spawn a prefab for the host!
            // SpawnPlayerPrefab(false, 0);
            isConnected = true;
        }

        public static void OnPlayerJoined(int connectionId)
        {

            NetworkManager.inst.StartCoroutine(SendPacketsLater(connectionId));
        }

        public static IEnumerator SendPacketsLater(int connectionId)
        {
            yield return new WaitForSeconds(0.1f);
            Debug.Log("OnPlayerJoined");
            //hostId = connectionId;

            // Send the introduction packet to this client
            Debug.Log("Sending out the intro packet");
            LobbyHostIntroductionPacket introPkt = new LobbyHostIntroductionPacket();
            connectedClients.Add(connectionId);
            introPkt.myPlayerId = nextPlayerIndex++;
            List<int> activePlayers = new List<int>();
            foreach(int i in connectedClients)
            {
                if(i != connectionId)
                    activePlayers.Add(connectionToPlayer[i]);
            }
            introPkt.activePlayerIds = activePlayers.ToArray();

            SpawnEntityPacket epkt = new SpawnEntityPacket();
            foreach (NetworkedEntity e in EntityManager.ents.Values)
            {
                if(e.GetComponent<LitePlayer>() == null)
                {
                    epkt.authority = e.GetComponent<NetworkAuthority>()?.owner?.id ?? 0;
                    epkt.entityId = (int) e.EntityIndex;
                    epkt.prefabId = e.GetComponent<NetworkIdentity>().id;
                    epkt.uniqueId = e.GetComponent<UniqueId>().uniqueId;
                    epkt.position = e.transform.position;
                    PacketSender.SendSpawnEntityPacket(epkt, connectionId);
                }
            }

            connectionToPlayer[connectionId] = nextPlayerIndex-1;

            Debug.Log("Spawining player prefab");
            SpawnPlayerPrefab(false, (nextPlayerIndex-1));

            Debug.Log("Actually sending the packet now");
            introPkt.instanceIdToEntity = new int[0];
            introPkt.preplacedInstanceIds = new long[0];
            LiteNetworkingGenerated.PacketSender.SendLobbyHostIntroductionPacket(introPkt, connectionId);
            Debug.Log("Done sending packet");
            // Send the player joined packet to all other clients
            foreach (int i in connectedClients)
            {
                if (i != connectionId)
                {
                    LobbyNewPlayerPacket pkt = new LobbyNewPlayerPacket();
                    pkt.newPlayerId = (nextPlayerIndex-1);
                    Debug.Log("Sending NP Packet");
                    PacketSender.SendLobbyNewPlayerPacket(pkt, i);
                    Debug.Log("Done Sending NP Packet");
                }
            }
            Debug.Log("Done SendPacketsLater");
        }

        private static void SpawnPlayerPrefab(bool isLocalPlayer, int playerId = 0, Vector3 position = new Vector3())
        {
            // Create the game object
            GameObject g = GameObject.Instantiate(NetworkManager.inst.playerPrefab);
            g.transform.position = position;

            // Install network agents
            LitePlayer p = g.AddComponent<LitePlayer>();
            Networking.players[playerId] = p;
            p.id = playerId;
            NetworkAuthority auth = g.AddComponent<NetworkAuthority>();

            if (!auth)
                auth = g.AddComponent<NetworkAuthority>();

            auth.owner = p;
            EntityManager.RegisterEntity(p.GetComponent<NetworkedEntity>(), playerId);

            if (isLocalPlayer)
            {
                Networking.localPlayer = p;
            }

            p.Init();

            if(Networking.isServer)
                ChunkHandler.i.OnPlayerLoad(p);

        }

        public static void CancelConnection()
        {
            byte error = 0;
            NetworkTransport.Disconnect(hostId, connectionId, out error);
            isConnected = false;
        }

        public static void CreatePlayer(bool isLocalPlayer, int id, Vector3 position = new Vector3())
        {
            SpawnPlayerPrefab(isLocalPlayer, id, position);
        }

        public static void Disconnect()
        {
            byte error;
            NetworkTransport.Disconnect(hostId, connectionId, out error);

            // Remove all players
            foreach (NetworkedEntity e in EntityManager.ents.Values)
            {
                GameObject.Destroy(e.gameObject);
            }
            EntityManager.ents.Clear();

            hostId = -1;
            isServer = false;
            currentLobby = null;
            nextPlayerIndex = 1;

            NetworkEvents.onDisconnect?.Invoke();
            connectionToPlayer.Clear();
            isConnected = false;
        }
        
        // Sorry this function is a bit weird
        // It takes a connection ID on the server,
        // a player ID on client
        public static void OnPlayerDisconnect(int connectionId)
        {
            if(Networking.isServer)
            {
                int playerId = connectionToPlayer[connectionId];
                NetworkedEntity e = EntityManager.ents[playerId];
                GameObject.Destroy(e.gameObject);
                EntityManager.ents.Remove(playerId);

                connectedClients.Remove(connectionId);
            }
            else
            {
                int playerId = connectionId;
                NetworkedEntity e = EntityManager.ents[playerId];
                GameObject.Destroy(e.gameObject);
                EntityManager.ents.Remove(playerId);

                connectedClients.Remove(connectionId);
            }
        }


        public static void ConnectToLobby(string lobbyIP)
        {
           /* if (Networking.IsBot())
            {
                // Fake a connection to itself
                if (currentLobby == null)
                {
                    Debug.LogError("Attempt to connect a bot to a server that isnt hosting a lobby");
                }
            }*/
            ConnectionConfig config = new ConnectionConfig();
            int myReliableChannelId = config.AddChannel(QosType.Reliable);
            int myUnreliableChannelId = config.AddChannel(QosType.Unreliable);
            HostTopology topology = new HostTopology(config, 10);
            /* int portCount = 1;
            while(hostId == -1 && portCount < 50)
            {
                hostId = NetworkTransport.AddHost(topology, 8080 + (portCount++));
            }*/
            hostId = NetworkTransport.AddHost(topology, 0);
            byte error;
            connectionId = NetworkTransport.Connect(hostId, "127.0.0.1", 8080, 0, out error);
            Debug.Log("My connection  id is " + connectionId);
            if ((NetworkError)error != NetworkError.Ok)
            {
                //Output this message in the console with the Network Error
                Debug.Log("There was this error : " + (NetworkError)error);
            }
            else
            {
                Debug.Log("Success???");
            }
        }   

        private static void OnPlayerConnectionEstablished()
        {

        }
    }

}