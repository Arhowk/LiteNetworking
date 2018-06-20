using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DebugLoadingScreen : MonoBehaviour, IAtlasSceneListener {
    public Slider progressSlider;
    public CanvasGroup cg;
    public Text continueText;

	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(transform.parent.gameObject);
        cg.alpha = 0;
        WorldAtlas.RegisterSceneListener(this);
	}

    public void OnSceneJobStart(int sceneId)
    {
        cg.alpha = 1;
    }

    public void OnSceneJobUpdate(float progress)
    {

    }

    public void OnSceneJobFinished()
    {

    }
		
	
}
