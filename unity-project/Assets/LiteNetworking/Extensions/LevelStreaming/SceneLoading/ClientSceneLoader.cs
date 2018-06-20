using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientSceneLoader : MonoBehaviour {
    public static ClientSceneLoader i;

    public void Awake()
    {
        i = this;
    }

    public static void LoadScene(int sceneId, bool isAdditive = false)
    {
        i.StartCoroutine(LoadSceneAsync(sceneId, isAdditive));
        SceneManager.LoadScene(sceneId);
    }

    private static IEnumerator LoadSceneAsync(int sceneId, bool isAdditive)
    {
        LoadSceneMode mode = isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneId, mode);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }   
}
