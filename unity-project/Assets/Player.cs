using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetworking;

public class Player : MonoBehaviour {
    public GameObject cubePrefab;
	// Use this for initialization
	void Start () {
		if(LobbyConnector.isServer)
        {
            GameObject prefab1 = Instantiate(cubePrefab, transform.position +new Vector3(0, 3f), Quaternion.identity);
            prefab1.GetComponent<NetworkedEntity>().Construct(GetComponent<LitePlayer>());
            GameObject prefab2 = Instantiate(cubePrefab, transform.position + new Vector3(3,0f), Quaternion.identity);
            prefab2.GetComponent<NetworkedEntity>().Construct(GetComponent<LitePlayer>());
            GameObject prefab3 = Instantiate(cubePrefab, transform.position + new Vector3(0, -3f), Quaternion.identity);
            prefab3.GetComponent<NetworkedEntity>().Construct(GetComponent<LitePlayer>());
            GameObject prefab4 = Instantiate(cubePrefab, transform.position + new Vector3(-3, 0f), Quaternion.identity);
            prefab4.GetComponent<NetworkedEntity>().Construct(GetComponent<LitePlayer>());
        }
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 delta = new Vector3();
        // print("Has local authroity : " + Networking.HasLocalAuthority(gameObject));
        if (Application.isFocused && Networking.HasLocalAuthority(gameObject))
        {
            if(Input.GetKey(KeyCode.LeftArrow))
            {
                delta.x = -1;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                delta.x = 1;
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                delta.z = 1;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                delta.z = -1;
            }

            transform.position = transform.position + delta * Time.deltaTime * 3;
        }
	}
}
