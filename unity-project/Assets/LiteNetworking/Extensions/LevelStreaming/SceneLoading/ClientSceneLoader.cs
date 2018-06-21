using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientSceneLoader : MonoBehaviour {
    public static ClientSceneLoader i;
    public bool waitingOnServer = false;
    public bool waitingOnClient = false;

    public void Start()
    {
        i = this;
    }

    public static void LoadScene(int sceneId, bool isAdditive = false)
    {
        // Clear all players
        LiteNetworking.EntityManager.DeleteAllEntities(true);

        // Start loading the game
        i.StartCoroutine(LoadSceneAsync(sceneId, isAdditive));
        //SceneManager.LoadScene(sceneId);
    }

    private static IEnumerator LoadSceneAsync(int sceneId, bool isAdditive)
    {
        LoadSceneMode mode = isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneId, mode);
        Debug.Log("LoadSceneAsync");
        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return new WaitForSeconds(0.1f);
            WorldAtlas.listeners.ForEach(a => a.OnSceneJobUpdate(asyncLoad.progress));
        }

        if(i.waitingOnClient)
        {
            Debug.Log("OnJobIsTOalFin!!!");
            i.waitingOnClient = false;
            WorldAtlas.listeners.ForEach(a => a.OnSceneJobFinished());
        }
        else
        {
            Debug.Log("WaitingONServer!!!");
            i.waitingOnServer = true;
            WorldAtlas.listeners.ForEach(a => a.OnSceneWaitingForServer());
        }
    }   

    public static void OnServerSceneJobFinished()
    {
        Debug.Log("OnServerJobFIn");
        if(i.waitingOnServer)
        {
            i.waitingOnServer = false;
            WorldAtlas.listeners.ForEach(a => a.OnSceneJobFinished());
        }
        else
        {
            i.waitingOnClient = true;
        }
    }
}
