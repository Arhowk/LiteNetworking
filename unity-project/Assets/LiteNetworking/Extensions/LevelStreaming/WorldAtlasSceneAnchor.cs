using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldAtlasSceneAnchor : MonoBehaviour {

    // Use this for initialization
    private void Start()
    {
        ChunkHandler.i.RegisterScene(this);
    }
}
