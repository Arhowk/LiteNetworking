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
    public AtlasWorld world;

    public List<LitePlayer> connectedPlayers = new List<LitePlayer>();
    public List<LiteNetworking.NetworkedEntity> connectedEntities = new List<LiteNetworking.NetworkedEntity>();

}
