using LiteNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SyncTransform))]
public class DebugCylinderDragger : NetworkedEntity {

    public Vector3 dir;

    public void Start()
    {
        int rand = Random.Range(0, 3);

        if (rand == 0) dir = Vector3.zero;
        else if (rand == 1) dir = Vector3.left;
        else dir = Vector3.right;
    }

    public void Update()
    {
        if(Networking.isServer)
             transform.position = transform.position + dir * Time.deltaTime * 0.3f;
    }
}
