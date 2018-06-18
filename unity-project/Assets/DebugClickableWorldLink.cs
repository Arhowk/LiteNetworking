using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugClickableWorldLink : MonoBehaviour {

    public DiscontinuousWorldLink activatedLink;
    public Collider clickable;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	    if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if(hit.collider == clickable)
                {
                    activatedLink.Activate();
                }
            }
        }
	}
}
