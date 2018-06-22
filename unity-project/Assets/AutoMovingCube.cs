using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetworking;

[RequireComponent(typeof(SyncTransform))]
public class AutoMovingCube : LiteNetworking.NetworkedEntity {

    [Networking.SyncOnRefresh]
    public int randomNumber;
	
	// Update is called once per frame
	void Update () {
        if(Networking.HasLocalAuthority(gameObject))
        {
            if (Networking.GetLocalPlayer().id == 0)
            {
                transform.position = transform.position + new Vector3(0.006f, 0, 0);
            }
            else
            {
                transform.position = transform.position + new Vector3(0, 0, 0.006f);
            }
        }
    }
}
