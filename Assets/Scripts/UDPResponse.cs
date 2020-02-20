using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UDPResponse : MonoBehaviour {

	public Text text = null;

	void Start () {
	
	}

	void Update () {
	
	}

	public void ResponseToUDPPacket(string fromIP, string fromPort, byte[] data)
	{
		string dataString = System.Text.Encoding.UTF8.GetString (data);
        Debug.Log(dataString);
		if (text != null) {
            Debug.Log("Set text");
			text.text = dataString;
		}
	}
}