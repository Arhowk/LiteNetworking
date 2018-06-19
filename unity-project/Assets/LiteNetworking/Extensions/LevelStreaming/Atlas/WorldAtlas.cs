﻿using LiteNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldAtlas : MonoBehaviour {

    public static bool enabled = true;

    public static WorldAtlas current;
    public Dictionary<int, AtlasWorld> worlds;
    public int lastChunkId = 0;

	// Use this for initialization
	void Start () {
        current = this;

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
            links = new List<WorldLinkData>() { coreToTop, coreToLeft }
        };

        AtlasWorld top = new AtlasWorld()
        {
            sceneId = 2,
            width = 1,
            height = 1,
            links = new List<WorldLinkData>() { topToCore }
        };

        AtlasWorld left = new AtlasWorld()
        {
            sceneId = 1,
            width = 1,
            height = 1,
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

        }
    }

    public AtlasWorld GetWorld(int sceneId)
    {
        return worlds[sceneId];
    }

    public void GoToScene(int sceneId, string offmeshLinkName)
    {
        SceneManager.LoadScene(sceneId);
    }
}