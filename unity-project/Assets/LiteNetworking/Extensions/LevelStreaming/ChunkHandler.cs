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
    
}


public class ChunkHandler : MonoBehaviour {

    public float gridWidth = 10f;
    public float gridHeight = 10f;

    public float maxXYValue = 30f;

    private int startingChunkX = 0, startingChunkY = 0, numColumns = 0, numRows = 0;

    public static ChunkHandler i;
    public WorldChunk[][] allChunks;
    public WorldChunk waitingChunk;

	// Use this for initialization
	void Start () {
        i = this;
        numColumns = (int)(maxXYValue / gridWidth);
        numRows = (int)(maxXYValue / gridHeight);

        allChunks = new WorldChunk[numColumns][];

        startingChunkX = (numColumns / 2);
        startingChunkY = (numRows / 2); 

        for(int i = 0; i < numColumns; i++)
        {
            allChunks[i] = new WorldChunk[numRows];
        }
	}
	
	// Update is called once per frame
	void Update () {    
		
	}

    public void GetNextChunk()
    {

    }

    public void RegisterScene(WorldAtlasSceneAnchor anchor)
    {
        if(waitingChunk == null)
        {
            Debug.LogError("Attempt to register a scene with no waiting chunk");
        }
        else
        {
            waitingChunk.anchor = anchor;
            waitingChunk.callback(waitingChunk);
            waitingChunk = null;
        }
    }

    public int FindChunk(int width, int height)
    {
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

                return j + i * numColumns;  
                exitLoop: continue;
            }
        }

        Debug.LogError("Out of world chunks!");
        return -1;  
    }

    public KeyValuePair<int,int> RequestChunk(int sceneId, System.Action<WorldChunk> callback)
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
                chunkSceneId = sceneId
            };


            for (int w = 0; w < width; w++)
            {
                for(int h = 0; h < height; h++)
                {
                    allChunks[w + chunkX][h + chunkY] = chunkObj;
                }
            }

            waitingChunk = chunkObj;

            return new KeyValuePair<int, int>(chunkX, chunkY);
        }
        else
        {
            Debug.LogError("Scene " + sceneId + " has no attached AtlasWorld");
            return new KeyValuePair<int, int>(-1,-1);
        }
    }
}
