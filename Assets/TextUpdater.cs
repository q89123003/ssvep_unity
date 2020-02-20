using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextUpdater : MonoBehaviour {

    private GameObject textObject;
    private GameObject text3dOjbect;
    private string text = "Hello";

    private string[] targetLabelMap = null;

	// Use this for initialization
	void Start () {
        //textObject = GameObject.Find("Text");
        text3dOjbect = GameObject.Find("Text 3D");
	}
	
	// Update is called once per frame
	void Update () {
        //textObject.GetComponent<Text>().text = text;
        text3dOjbect.GetComponent<TextMesh>().text = text;
	}

    public void setText(string txt)
    {
        text = txt;
    }

    public void setText(int target)
    {
        if (target == 0 || targetLabelMap == null)
        {
            text = ".";
        }
        else
        {
            text = targetLabelMap[target - 1];
        }
    }

    public void setTargetLabelMap(string[] map)
    {
        targetLabelMap = map;
    }
}
