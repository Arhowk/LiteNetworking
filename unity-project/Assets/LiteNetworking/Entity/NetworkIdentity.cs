using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetworking;

[ExecuteInEditMode()]
public class NetworkIdentity : MonoBehaviour {

    public GameObject connectedPrefab;
    public int networkIdentity = -1;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (connectedPrefab != null && networkIdentity == -1)
        {
            networkIdentity = Random.Range(0, System.Int32.MaxValue);
            EntityManager.RegisterPrefab(connectedPrefab, networkIdentity);
        }

    }

    public void OnStartClient()
    {
        if(networkIdentity != -1)
        {
            EntityManager.RegisterPrefab(connectedPrefab, networkIdentity);
        }
    }
}
