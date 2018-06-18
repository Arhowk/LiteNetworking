using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldAtlas : MonoBehaviour {
    public static WorldAtlas current;
	// Use this for initialization
	void Start () {
        current = this;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void GoToScene(int sceneId, string offmeshLinkName)
    {

    }
}
