using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LiteNetworking
{
    public class EntityManager 
    {
        public static Dictionary<long, GameObject> registeredPrefabs = new Dictionary<long, GameObject>();
        public static Dictionary<long, NetworkedEntity> registeredPreplacedEnts = new Dictionary<long, NetworkedEntity>();

        public static Dictionary<int, NetworkedEntity> ents = new Dictionary<int, NetworkedEntity>();
        private static int nextNormalEnt = 4096;
        private static int nextTempIndex = System.Int32.MaxValue - 1;

        static EntityManager()
        {
            NetworkEvents.onDisconnect += DeleteAllEntities;
        }

        public static void DeleteAllEntities()
        {
            DeleteAllEntities(false);
        }

        public static void DeleteAllEntities(bool keepLocalPlayer)
        {
            LitePlayer localHero = keepLocalPlayer ? Networking.GetLocalPlayer() : null;
            foreach(KeyValuePair<int, NetworkedEntity> pairs in ents)
            {
                if(pairs.Value != localHero) GameObject.Destroy(pairs.Value.gameObject);
            }
            ents.Clear();
            if (keepLocalPlayer) ents[localHero.id] = localHero;
        }

        public static uint RegisterEntity(NetworkedEntity e)
        {
            ents[nextNormalEnt++] = e;
            e.SetEntityIndex(nextNormalEnt - 1, false);
            return (uint) (nextNormalEnt - 1);
        }
        public static void RegisterEntity(NetworkedEntity e, int id)
        {
            e.SetEntityIndex(id, false);
            ents[id] = e;
        }


        public static NetworkedEntity GetEntity(uint index)
        {
            if(ents.ContainsKey((int)index))
            {
                return ents[(int)index];
            }
            else
            {
                Debug.Log("entity " + index + " does not exist");
                    return null; 
            }
        }

        public static void RegisterPrefab(GameObject g, long identity)
        {
            registeredPrefabs[identity] = g;
        }

        public static NetworkedEntity RegenerateEntity(int id, long prefab, int authority, Vector3 pos, long uniqueId = 0)
        {
            // TODO: Separate preplaced entities from dynamically created ents
            if(uniqueId != 0)
            {
                if(registeredPreplacedEnts.ContainsKey(uniqueId))
                {
                    NetworkedEntity e = registeredPreplacedEnts[uniqueId];

                    e.SetEntityIndex(id);
                    e.GetComponent<NetworkAuthority>()?.SetAuthority(authority);

                    registeredPreplacedEnts.Remove(uniqueId);

                    return e;
                }
            }

            if(registeredPrefabs.ContainsKey(prefab))
            {
                GameObject newObject = MonoBehaviour.Instantiate(registeredPrefabs[prefab], pos, Quaternion.identity);

                newObject.GetComponent<NetworkedEntity>().SetEntityIndex(id);
                newObject.GetComponent<NetworkAuthority>()?.SetAuthority(authority);

                return newObject.GetComponent<NetworkedEntity>();
            }
            else
            {
                Debug.LogError("Entity manager does not have prefab " + prefab);
            }
            return null;
        }

        public static void OnEntityAwake(NetworkedEntity e)
        {
            if(!Networking.isConnected)
            {
                registeredPreplacedEnts[e.GetComponent<UniqueId>().uniqueId] = e;
            }
            //ents[nextTempIndex--] = e;
        }

        public static void StartServer()
        {
            // Swap all temporary prefabs to registered entities
            foreach(NetworkedEntity e in registeredPreplacedEnts.Values)
            {
                RegisterEntity(e);
            }

            registeredPreplacedEnts.Clear();
        }

        public static NetworkedEntity SpawnEntity()
        {

            return null;
        }
    }
}
