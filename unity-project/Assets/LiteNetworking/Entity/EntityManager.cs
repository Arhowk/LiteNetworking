using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LiteNetworking
{
    public class EntityManager 
    {
        public static Dictionary<int, GameObject> registeredPrefabs = new Dictionary<int, GameObject>();

        public static Dictionary<int, NetworkedEntity> ents = new Dictionary<int, NetworkedEntity>();
        private static int nextNormalEnt = 4096;

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

        public static void RegisterPrefab(GameObject g, int identity)
        {
            registeredPrefabs[identity] = g;
        }

        public static NetworkedEntity RegenerateEntity(int id, int prefab, int authority, Vector3 pos)
        {
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
    }
}
