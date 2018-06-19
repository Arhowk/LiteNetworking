using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkHandler : MonoBehaviour {

    public float gridWidth = 10f;
    public float gridHeight = 10f;

    public float maxXYValue = 30f;

    private int startingChunkX = 0, startingChunkY = 0, numColumns = 0, numRows = 0;

    public static ChunkHandler i;
    public WorldChunk[][] allChunks;

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

    public int FindChunk(int width, int height)
    {
        for(int i = 0; i < numRows; i++)
        {
            for(int j = 0; j < numColumns; j++)
            {
                if(allChunks[j][i] == null)
                {
                    return j + i * numColumns;
                }
            }
        }

        Debug.LogError("Out of world chunks!");
        return -1;  
    }

    public void RequestChunk(int sceneId, System.Action callback)
    {
        // Try get chunk size
        AtlasWorld world = WorldAtlas.current.GetWorld(sceneId); //this naming convention is awful

        if(world != null)
        {
            int width = world.width, height = world.height;
            int chunk = FindChunk(width, height);
             
        }
        else
        {
            Debug.LogError("Scene " + sceneId + " has no attached AtlasWorld");
        }
    }
}
