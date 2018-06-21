using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldAtlasSceneAnchor : MonoBehaviour {

    // Use this for initialization
    private void Start()
    {
        if(ChunkHandler.i == null)
        {
            ChunkHandler.waitingAnchor = this;
        }
        else
        {
            ChunkHandler.i.RegisterScene(this);
        }
    }
}
