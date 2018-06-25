using LiteNetworking;
using LiteNetworkingGenerated;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Client --> Server when he is entering a new area
public class RequestChunkPacket : LiteNetworking.LitePacket
{
    public int sceneId;

    public override void Execute()
    {
        
    }
}

// Client --> Server when he fully enters a new area
public class OnSceneChangedPacket : LiteNetworking.LitePacket
{
    public int sceneId;
    public string offmeshLinkName;

    public override void Execute()
    {
        WorldAtlas.current.MovePlayerToScene(GetExecutingClient(), sceneId, null, chk =>
        {
            SpawnEntityPacket epkt = new SpawnEntityPacket();
            foreach (NetworkedEntity e in EntityManager.ents.Values)
            {
                if (e.GetComponent<LitePlayer>() == null && e.GetChunkId() == chk.chunk)
                {
                    epkt.authority = e.GetComponent<NetworkAuthority>()?.owner?.id ?? 0;
                    epkt.entityId = (int)e.EntityIndex;
                    epkt.prefabId = e.GetComponent<UniqueId>().prefabId;
                    epkt.uniqueId = e.GetComponent<UniqueId>().uniqueId;
                    epkt.position = e.transform.position;
                    PacketSender.SendSpawnEntityPacket(epkt, GetExecutingClient().GetConnectionId());
                }
            }
        });


    }
}

public class OnSceneChangedClient : LiteNetworking.LitePacket
{
    public int[] playersInScene;

    [LevelStreaming.Position]
    public Vector3[] playerPositions;

    public override void Execute()
    {
        Debug.Log("OnSceneChangedClient");
        // Make new players
        Debug.Log("The amount of players currently in the scene is " + playersInScene.Length);
        for(int i = 0; i < playersInScene.Length; i++)
        {
            LiteNetworking.LobbyConnector.CreatePlayer(false, playersInScene[i], playerPositions[i]);
        }
        ClientSceneLoader.OnServerSceneJobFinished();
    }
}


public class ChunkHandler : MonoBehaviour {

    public float gridWidth = 10f;
    public float gridHeight = 10f;

    public float maxXYValue = 30f;

    private int startingChunkX = 0, startingChunkY = 0, numColumns = 0, numRows = 0;

    public static ChunkHandler i;
    public WorldChunk[][] allChunks;
    public static WorldChunk waitingChunk;
    public static WorldAtlasSceneAnchor waitingAnchor;
    public Dictionary<int, List<WorldChunk>> chunkRefs;
   

	// Use this for initialization
	void Awake () {
        i = this;
        numColumns = (int)(maxXYValue / gridWidth);
        numRows = (int)(maxXYValue / gridHeight);

        allChunks = new WorldChunk[numColumns][];
        chunkRefs = new Dictionary<int, List<WorldChunk>>();

        startingChunkX = (numColumns / 2);
        startingChunkY = (numRows / 2); 

        for(int i = 0; i < numColumns; i++)
        {
            allChunks[i] = new WorldChunk[numRows];
        }

        if(waitingAnchor != null)
        {
            MakeChunkForDefaultScene(waitingAnchor);
        }
        else
        {
            Debug.Log("No waiting chunk!");
        }
	}
	
	// Update is called once per frame
	void Update () {    
		
	}

    public void GetNextChunk()
    {

    }

    public Vector3 GetChunkOffset(int chunk)
    {
        return new Vector3(gridWidth * chunk,0,0);
    }

    public WorldChunk GetChunk(int chunk)
    {
        return allChunks[chunk % numColumns][chunk / numColumns];
    }

    public void OnPlayerLoad(LitePlayer player)
    {
        allChunks[0][0].connectedPlayers.Add(player);
    }

    public void MakeChunkForDefaultScene(WorldAtlasSceneAnchor anch)
    {
        Debug.Log("MakeChunkForDefaultScene!!!");
        WorldChunk chunk = new WorldChunk()
        {
            x = 0,
            y = 0,
            width = 1,
            height = 1,
            chunkSceneId = 0,
            anchor = anch
        };

        allChunks[0][0] = chunk;
        chunkRefs[0] = new List<WorldChunk>() { chunk };

    }

    public void RegisterScene(WorldAtlasSceneAnchor anchor)
    {
        Debug.Log("RegisterScene!");
        if(waitingChunk == null)
        {
            Debug.Log("Make Default!");
            // Debug.LogError("Attempt to register a scene with no waiting chunk");
            MakeChunkForDefaultScene(anchor);
        }
        else
        {
            Debug.Log("Callback!");
            int x = waitingChunk.x, y = waitingChunk.y;
            float offsetX = x * gridWidth, offsetY = y * gridHeight;
            Debug.Log("The chunk coords are " + x + " : " + y);

            anchor.gameObject.transform.position = new Vector3(offsetX, 0, offsetY);

            waitingChunk.anchor = anchor;
            waitingChunk.callback(waitingChunk);
            waitingChunk = null;
        }
    }

    public int FindChunk(int width, int height)
    {
        Debug.Log("Trying to find a chunk for size " + width + ":" + height);
        for(int i = 0; i < numRows; i++)
        {
            for(int j = 0; j < numColumns; j++)
            {

                // Check the chunk from ij to i+w,j+h
                for(int w = 0; w < width; w++)
                {
                    for(int h = 0; h < height; h++)
                    {
                        if (allChunks[j+w][i+h] != null) goto exitLoop;
                    }
                }
                Debug.Log("Found a chunk at " + i + ":" + j);
                return j + i * numColumns;  
                exitLoop: continue;
            }
        }

        Debug.LogError("Out of world chunks!");
        return -1;  
    }

    public WorldChunk GetChunkForPlayer(LitePlayer player, int sceneId)
    {
        if(!chunkRefs.ContainsKey(sceneId))
        {
            Debug.Log("ChunkRefs doesnt have scene");
            return null;
        }

        foreach(WorldChunk chk in chunkRefs[sceneId])
        {
            if (true) return chk;
            foreach(LitePlayer plyr in chk.connectedPlayers)
            {
                if (plyr.id == player.id)
                {
                    return chk;
                }
            }
        }

        return null;
    }

    public WorldChunk RequestChunk(int sceneId, System.Action<WorldChunk> callback = null)
    {
        // Try get chunk size
        AtlasWorld world = WorldAtlas.current.GetWorld(sceneId); //this naming convention is awful

        if(world != null)
        {
            int width = world.width, height = world.height;
            int chunk = FindChunk(width, height);
            int chunkX = chunk % numColumns;
            int chunkY = chunk / numColumns;
            WorldChunk chunkObj = new WorldChunk()
            {
                x = chunkX,
                y = chunkY,
                width = width,
                height = height,
                chunkSceneId = sceneId,
                world = world
            };

            if(!chunkRefs.ContainsKey(sceneId))
            {
                chunkRefs[sceneId] = new List<WorldChunk>() { chunkObj };
            }
            else
            {
                chunkRefs[sceneId].Add(chunkObj);
            }

            for (int w = 0; w < width; w++)
            {
                for(int h = 0; h < height; h++)
                {
                    allChunks[w + chunkX][h + chunkY] = chunkObj;
                }
            }

           // callback(chunkObj);
            waitingChunk = chunkObj;
            chunkObj.callback = callback;

            ServerSceneLoader.LoadScene(sceneId);

            return chunkObj;
        }
        else
        {
            Debug.LogError("Scene " + sceneId + " has no attached AtlasWorld");
            return null;
        }
    }

    public void TryAddPlayerToScene(LitePlayer p, int sceneId)
    {
        AtlasWorld world = WorldAtlas.current.GetWorld(sceneId); //this naming convention is awful

        // Remove player from his current chunk
        WorldChunk currChunk = GetChunk(p.GetChunkId());
        currChunk.connectedPlayers.Remove(p);

        if(world.instanced)
        {
            // {Todo}
        }
        else
        {
            List<WorldChunk> chunks = chunkRefs[sceneId];

            if(chunks == null)
            {
                WorldChunk chunk = RequestChunk(sceneId);
                chunk.connectedPlayers.Add(p);
            }
            else
            {
                chunks[0].connectedPlayers.Add(p);
            }
        }
    }
}
