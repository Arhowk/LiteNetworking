using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldChunk  {
    public bool isDefaultChunk = false;
    public int chunkSceneId;
    public int x, y;
    public int width, height;
    public GameObject root;
    public WorldAtlasSceneAnchor anchor;
    public System.Action<WorldChunk> callback;

    public List<LitePlayer> connectedPlayers;
    public List<LiteNetworking.NetworkedEntity> connectedEntities;


    public static void LoadScene(int sceneId, string hook)
    {
            
    }
}
