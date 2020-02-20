using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System;
using System.IO;
using System.Text;

#if !UNITY_EDITOR
    using Windows.Networking;
    using Windows.Networking.Sockets;
    using Windows.Storage.Streams;
#endif

public class Tutorial : MonoBehaviour {

    static int LPT_EVENT_MARKER = 1;
    static int TCP_EVENT_MARKER = 2;

    int EVENT_MARKER_TYPE = LPT_EVENT_MARKER;

    TextToSpeech textToSpeech = null;
    StimulusManager stimulusManager;
    float targetTime = 0.0f;
    int state;

    int trainingTarget;
    int numPerRow = 1;
    int numPerColumn = 1;
    // Use this for initialization

#if !UNITY_EDITOR
     string ip = "192.168.1.100";
     string port = "9998";
     StreamSocket socket;
#endif

    void Start()
    {
        targetTime = 2f;
        state = 0;
        trainingTarget = 0;
        
        var soundManager = GameObject.Find("Audio Manager");
        textToSpeech = soundManager.GetComponent<TextToSpeech>();
        textToSpeech.Voice = TextToSpeechVoice.Zira;
        textToSpeech.StartSpeaking("Hello, welcome to the Steady State Visually Evoked Potential experiment!");

        var stimulusManagerObject = GameObject.Find("Stimulus Manager");
        stimulusManager = stimulusManagerObject.GetComponent<StimulusManager>();
        //stimulusManager.InitialTargets(numPerColumn, numPerRow, -1.5f, 7f, 8f, 1f);
        stimulusManager.InitialTargets(numPerColumn, numPerRow, 0f, 0f, 8f, 1f);
    }

    // Update is called once per frame
    void Update () {
        targetTime -= Time.deltaTime;

        if (targetTime <= 0.0f && !textToSpeech.IsSpeaking()) {
            if (state == 0)
            {
                ++state;
                string s = "Please follow the red target.";
                Debug.Log(s);
                textToSpeech.StartSpeaking(s);
                targetTime = 1f;
            }
        }
        if (targetTime <= 0.0f && state == 1)
        {
            //stimulusManager.GazeShift(trainingTarget);
            stimulusManager.GazeShift();

            if (EVENT_MARKER_TYPE == TCP_EVENT_MARKER)
            {
#if !UNITY_EDITOR
        Connect();
#endif
            }
            else if (EVENT_MARKER_TYPE == LPT_EVENT_MARKER)
            {
                ParallelPortManager.Out32_x64(ParallelPortManager.address, 0);
            }

            if (++trainingTarget == numPerColumn * numPerRow)
                trainingTarget = 0;
            targetTime = 1f;
            state = 2;
        }
        if (targetTime <= 0.0f && state == 2)
        {
            byte[] msg = new byte[1];
            msg[0] = 1;

            stimulusManager.StartStimulating();

            targetTime = 2f;
            state = 1;

            if (EVENT_MARKER_TYPE == TCP_EVENT_MARKER)
            {
#if !UNITY_EDITOR
            SendMessage(msg);
#endif
            }
            else if (EVENT_MARKER_TYPE == LPT_EVENT_MARKER)
            {
                ParallelPortManager.Out32_x64(ParallelPortManager.address, 1);
            }
        }
    }

#if !UNITY_EDITOR
private async void Connect()
{
    HostName hostName;
    try
    {
        hostName = new HostName(ip);
    }
    catch (ArgumentException e)
    {
        Debug.Log(e.ToString());
        return;
    }
         
    try{
        socket = new StreamSocket();
        await socket.ConnectAsync(hostName, port);
    }
    catch (Exception e)
    {
        Debug.Log(e.ToString());
    }

    Debug.Log("Connect complete!");
}
#endif

#if !UNITY_EDITOR
 private async void SendMessage(byte[] bmsg)
 {
     try
     {
        DataWriter dw = new DataWriter(socket.OutputStream);
        dw.WriteBytes(bmsg);
        await dw.StoreAsync();

        socket.Dispose();
        socket = null;
     }
     catch (Exception e)
     {
        Debug.Log(e.ToString());
     }
 }
#endif
}
