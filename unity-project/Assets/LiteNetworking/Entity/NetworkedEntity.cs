using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using LiteNetworkingGenerated;
using System;

namespace LiteNetworking
{   
    [RequireComponent(typeof(NetworkIdentity))]
    public partial class NetworkedEntity : MonoBehaviour
    {
        [SerializeField]
        public uint EntityIndex;

        public void Awake()
        {
            EntityManager.OnEntityAwake(this);
        }

        public void Construct(LitePlayer owner, int entid = -1)
        {
            EntityIndex = EntityManager.RegisterEntity(this);

            GetComponent<NetworkAuthority>().owner = owner;

            SpawnEntityPacket pkt = new SpawnEntityPacket();
            pkt.authority = owner.id;
            pkt.entityId = (int)EntityIndex;
            pkt.position = transform.position;
            pkt.prefabId = GetComponent<NetworkIdentity>().id;

            if(WorldAtlas.enabled)
            {
                this.loadedChunkId = owner.GetChunkId();

                foreach(LitePlayer player in Networking.players.Values)
                {
                    if(player.GetChunkId() == loadedChunkId)
                    {
                        PacketSender.SendSpawnEntityPacket(pkt, player.GetConnectionId());
                    }
                }
            }
            else
            {
                PacketSender.SendSpawnEntityPacket(pkt);
            }

        }


        public void SetEntityIndex(int index, bool registerEntity = true)
        {
            EntityIndex = (uint) index;

            if(registerEntity) EntityManager.RegisterEntity(this, index);
        }

        public static void RegenerateEntityFromId(int prefabIndex)
        {
            GameObject prefab = NetworkManager.inst.temporaryEntityPrefabs[prefabIndex];

            Instantiate(prefab);
        }
    }
}
