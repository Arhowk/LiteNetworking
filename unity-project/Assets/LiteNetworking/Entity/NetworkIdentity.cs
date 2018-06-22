using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetworking;

[RequireComponent(typeof(UniqueId))]
public class NetworkIdentity : MonoBehaviour {

    public GameObject connectedPrefab;
    private UniqueId _id;

    public long id
    {
        get
        {
            if (_id == null) _id = GetComponent<UniqueId>();
                
            return _id.uniqueId;
        }
    }
    

    // Use this for initialization
    void Awake () {
        _id = GetComponent<UniqueId>();
	}

    public void OnStartClient()
    {
        EntityManager.RegisterPrefab(connectedPrefab, id);
    }
}
