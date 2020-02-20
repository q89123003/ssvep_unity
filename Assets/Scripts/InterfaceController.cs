using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceController : MonoBehaviour {

    public List<GameObject> blendables;
    public bool blendingEnabled;
	
	public void BlendOut()
    {
        if (!blendingEnabled) return; 
        foreach (GameObject blendable in blendables)
        {
            blendable.SetActive(false);
        }
    }

    public void BlendIn()
    {
        if (!blendingEnabled) return;
        foreach (GameObject blendable in blendables)
        {
            blendable.SetActive(true);
        }
    }


}
