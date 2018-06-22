using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugComponent : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Debug.Log("Start Component " + gameObject.GetInstanceID());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
