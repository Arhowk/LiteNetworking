using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldAtlasSceneAnchor : MonoBehaviour {

    // Use this for initialization
    private void Awake()
    {
        WorldAtlas.current.RegisterScene(this);
    }
}
