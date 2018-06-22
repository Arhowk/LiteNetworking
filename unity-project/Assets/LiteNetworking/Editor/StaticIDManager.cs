using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class StaticIDManager
{
    public static Dictionary<long, UniqueId> ids = new Dictionary<long, UniqueId>();

    public static bool RegisterID(long id, UniqueId obj)
    {
        if(ids.ContainsKey(id))
        {
            Debug.Log("Checking if the existing key matches this UniqueId");
            return ids[id] == obj;
        }
        ids[id] = obj;
        return true;
    }
}