using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscontinuousWorldLink : WorldLink
{
    public string thisLinkName;
    public int targetSceneId;
    public string targetLinkName;
    
    public void Activate()
    {
        WorldAtlas.current.GoToScene(targetSceneId, targetLinkName);
    }
}
