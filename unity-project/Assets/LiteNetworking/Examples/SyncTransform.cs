using LiteNetworking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DigitalRuby.Tween;

public class SyncTransformPacket : LitePacket {

    public NetworkedEntity t;

    public Vector3 position, euler, scale;

    public override void Execute()
    {
        NetworkedEntity tx = t;
        uint myEntityIndex = tx.EntityIndex;
        if (t == null) return;
     //   Debug.Log("Attempting to move ent " + t.EntityIndex + " : " + position);

        Vector3 endPos = position;
        Vector3 endEuler = euler;
        Vector3 endScale = scale;

        Vector3 startPos = tx.transform.position;
        Vector3 startScale = tx.transform.localScale;
        Vector3 startRotate = tx.transform.eulerAngles;


        tx.gameObject.Tween("SyncTransfom" + t.EntityIndex, 0f, 1f, 0.21f, TweenScaleFunctions.Linear, (x) =>
        {
         //   Debug.Log("Now tx is " + tx.EntityIndex);
        //    Debug.Log("But I wanted " + myEntityIndex);
            tx.transform.position = startPos + (endPos - startPos) * x.CurrentProgress;
            tx.transform.eulerAngles = startRotate + (endEuler - startRotate) * x.CurrentProgress;
            tx.transform.localScale = startScale + (endScale - startScale) *  x.CurrentProgress;
        },
        (x) =>
        {
            tx.transform.position = endPos;
            tx.transform.eulerAngles = endEuler;
            tx.transform.localScale = endScale;
        });
    }

    public override bool Verify()
    {
        return CheckAuthority(GetExecutingClient());
    }
}



[RequireComponent(typeof(LiteNetworking.NetworkedEntity))]
public class SyncTransform : MonoBehaviour
{
    public float dir;
    public float dir2;
    public SyncTransformPacket pkt = new SyncTransformPacket();
    public float timeElapsed = 0;
    public void Start()
    {
        pkt.t = GetComponent<NetworkedEntity>();
    }
    public void Update()
    {
        //print("my id is " + GetComponent<NetworkedEntity>().EntityIndex);
        if(Networking.HasLocalAuthority(gameObject))
        {
           
            timeElapsed += Time.deltaTime;
            if(timeElapsed > 0.2f)
            {
               // print("AUTH: " + GetComponent<NetworkedEntity>().EntityIndex);
              //  print("IS HOST " + (GetComponent<NetworkAuthority>().owner.id == 0));
                timeElapsed = 0f;
                pkt.position = pkt.t.transform.position;
                pkt.euler = pkt.t.transform.eulerAngles;
                pkt.scale = pkt.t.transform.localScale;
                //  Networking.SendSyncTransformPacket(pkt);
                // print("Try send sync transform packet");
                LiteNetworkingGenerated.PacketSender.SendSyncTransformPacket(pkt);
            }
        }
    }
}