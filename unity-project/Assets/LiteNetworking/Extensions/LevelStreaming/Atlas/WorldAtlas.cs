using LiteNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using LiteNetworkingGenerated;

public class WorldAtlas : MonoBehaviour {

    public static bool enabled = true;

    public static WorldAtlas current;
    public Dictionary<int, AtlasWorld> worlds;
    public int lastChunkId = 0;
    public static List<IAtlasSceneListener> listeners = new List<IAtlasSceneListener>();

	// Use this for initialization
	void Start () {
        current = this;

	}

    public static void RegisterSceneListener(IAtlasSceneListener listener)
    {
        listeners.Add(listener);
    }

    public void GenerateDebugData()
    {
        worlds = new Dictionary<int, AtlasWorld>();

        // Generate the links
        WorldLinkData leftToCore = new WorldLinkData()
        {
            name = "toLobbyFromLeft",
            isDiscontinuous = true,
            approxLocation = Vector3.zero,
            connectedLinks = new List<WorldLinkData>()
        };

        WorldLinkData coreToLeft = new WorldLinkData()
        {
            name = "toLeft",
            isDiscontinuous = true,
            approxLocation = Vector3.zero,
            connectedLinks = new List<WorldLinkData>()
        };
        
        WorldLinkData topToCore = new WorldLinkData()
        {
            name = "toLobbyFromTop",
            isDiscontinuous = true,
            approxLocation = Vector3.zero,
            connectedLinks = new List<WorldLinkData>()
        };

        WorldLinkData coreToTop = new WorldLinkData()
        {
            name = "toTop",
            isDiscontinuous = true,
            approxLocation = Vector3.zero,
            connectedLinks = new List<WorldLinkData>()
        };

        // Connect links

        leftToCore.connectedLinks.Add(coreToLeft);
        coreToLeft.connectedLinks.Add(leftToCore);

        topToCore.connectedLinks.Add(coreToTop);
        coreToTop.connectedLinks.Add(topToCore);

        // Generate the worlds
        AtlasWorld core = new AtlasWorld()
        {
            sceneId = 0,
            width = 1,
            height = 1,
            instanced=false,
            links = new List<WorldLinkData>() { coreToTop, coreToLeft }
        };

        AtlasWorld top = new AtlasWorld()
        {
            sceneId = 2,
            width = 1,
            height = 1,
            instanced=true,
            links = new List<WorldLinkData>() { topToCore }
        };

        AtlasWorld left = new AtlasWorld()
        {
            sceneId = 1,
            width = 1,
            height = 1,
            instanced=false,
            links = new List<WorldLinkData>() { leftToCore }
        };


        // Add them
        worlds[core.sceneId] = core;
        worlds[top.sceneId] = top;
        worlds[left.sceneId] = left;
    }

    public void RegisterScene(WorldAtlasSceneAnchor anch)
    {
        if(Networking.isServer)
        {
            // Assign this world to the currently loading chunk

            // 
        }
    }

    public AtlasWorld GetWorld(int sceneId)
    {
        return worlds[sceneId];
    }

    public void GoToScene(int sceneId, string offmeshLinkName)
    {
        if(Networking.isServer)
        {
            Debug.LogError("Use MovePlayerToScene on the server to properly handle chunking");
            return;
        }
        
        // Start the client jobs
        listeners.ForEach(a => a.OnSceneJobStart(sceneId));
        ClientSceneLoader.LoadScene(sceneId);

        // Request the data from the server
        OnSceneChangedPacket pkt = new OnSceneChangedPacket();
        pkt.sceneId = sceneId;
        pkt.offmeshLinkName = offmeshLinkName;
        PacketSender.SendOnSceneChangedPacket(pkt);
    }

    public void PrepareSceneForPlayer(LitePlayer player, int sceneId, WorldLink fromLink, System.Action callbackOnComplete)
    {
        if(!Networking.isServer)
        {
            Debug.LogError("Attempt to call server fn on client");
            return;
        }

        KeyValuePair<int, int> xy = ChunkHandler.i.RequestChunk(sceneId, (chunk) =>
        {
            GameObject chunkRoot = chunk.root; // Wow.
        });
    }

    public void RemovePlayerFromScene(LitePlayer player, int sceneId)
    {
        if (!Networking.isServer)
        {
            Debug.LogError("Attempt to call server fn on client");
            return;
        }
    }

    public void MovePlayerToScene(LitePlayer player, int sceneId, WorldLink fromLink)
    {
        if (!Networking.isServer)
        {
            Debug.LogError("Attempt to call server fn on client");
            return;
        }

        WorldChunk chunk = ChunkHandler.i.GetChunkForPlayer(player, sceneId);

        if(chunk == null)
        {
            PrepareSceneForPlayer(player, sceneId, fromLink, () => 
            {

            });
        }
    }

}