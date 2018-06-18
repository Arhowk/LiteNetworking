using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkAuthority : MonoBehaviour {

    public LitePlayer owner;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetAuthority(int authority)
    {
        owner = LiteNetworking.Networking.GetPlayer(authority);
    }
}
