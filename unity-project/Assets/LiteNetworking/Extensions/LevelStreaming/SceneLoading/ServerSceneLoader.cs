using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerSceneLoader : MonoBehaviour
{
    public static ServerSceneLoader i;

    public void Awake()
    {
        i = this;
    }

    public static void LoadScene(int sceneId)
    {
        i.StartCoroutine(LoadSceneAsync(sceneId));
        SceneManager.LoadScene(sceneId);
    }

    private static IEnumerator LoadSceneAsync(int sceneId)
    {
        LoadSceneMode mode = LoadSceneMode.Additive;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneId, mode);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
