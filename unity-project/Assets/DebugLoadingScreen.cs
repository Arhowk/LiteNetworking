using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DebugLoadingScreen : MonoBehaviour, IAtlasSceneListener {
    public Slider progressSlider;
    public CanvasGroup cg;
    public Text continueText;
    public bool ready = false;

	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(transform.parent.gameObject);
        cg.alpha = 0;
        WorldAtlas.RegisterSceneListener(this);
	}

    public void Update()
    {
        if(ready)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                ready = false;
                cg.alpha = 0;
            }
        }
    }

    public void OnSceneJobStart(int sceneId)
    {
        print("OnSceneJobStart");
        cg.alpha = 1;
    }

    public void OnSceneJobUpdate(float progress)
    {
        progressSlider.value = progress;
        print("OnSceneJobUpdate");
    }


    public void OnSceneJobFinished()
    {
        print("OnSceneJobFin");
        continueText.text = "Press space to continue";
        ready = true;
    }

    public void OnSceneWaitingForServer()
    {
        print("OnSceneJobWaitServer");
        continueText.text = "Waiting On Server";
    }
}
