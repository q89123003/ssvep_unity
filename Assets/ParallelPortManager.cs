using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;


public class ParallelPortManager : MonoBehaviour {

    public static int address = 57336;

    //int cycleCount = 0;

    [DllImport("inpoutx64", EntryPoint = "Out32")]
    public static extern void Out32_x64(int adress, int value);

    [DllImport("inpoutx64", EntryPoint = "IsInpOutDriverOpen")]
    private static extern UInt32 IsInpOutDriverOpen_x64();
    // Use this for initialization
    void Start () {
        Debug.Log("Result of opening driver: " + IsInpOutDriverOpen_x64().ToString());
    }
	
	// Update is called once per frame
	void Update () {
        /*
        cycleCount += 1;

        if (cycleCount == 150)
        {
            Debug.Log("Up");
            Out32_x64(address, 1);
        }
        
        else if (cycleCount == 160)
        {
            Debug.Log("Down");
            Out32_x64(address, 0);
            cycleCount = 0;
        }
        */   
    }
}
