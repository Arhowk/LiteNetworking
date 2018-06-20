using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAtlasSceneListener  {
    void OnSceneJobFinished();
    void OnSceneJobStart(int sceneId);
    void OnSceneWaitingForServer();
    void OnSceneJobUpdate(float progress);
}
