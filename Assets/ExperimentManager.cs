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
using LSL;

#if !UNITY_EDITOR
    using Windows.Networking;
    using Windows.Networking.Sockets;
    using Windows.Storage.Streams;
#endif

public class ExperimentManager : MonoBehaviour {

    static int LPT_EVENT_MARKER = 1;
    static int TCP_EVENT_MARKER = 2;
    static int LSL_EVENT_MARKER = 3;

    int EVENT_MARKER_TYPE = LSL_EVENT_MARKER;

    liblsl.StreamInfo info;
    liblsl.StreamOutlet outlet;

   StimulusManager stimulusManager;
    TextUpdater textUpdater;
    float targetTime = 0.0f;
    int state;
    int waitKeyState;
    int trainingTarget;
    string s;

    float[] PredefinedFreqs = {7.5f, 12f, 20f};
    float HighCarrierFreq = 39f; //43f
    float[] HighPredefinedFreq = { 30f, 29f, 28f, 27f }; //{ 34f, 33f, 32f, 31f };
    //int[] conditions = { 1, 2, 3, 4, 5, 6, 7, 8 };
    int[] conditions = { 1, 1, 1, 1};
    int[] orders;
    int conditionCount = 0;
    int trialCount = 0;
    int blockCount = 0;
    float z_pos = 60;
    bool setTextFlag = false;
    bool practiceFlag = false;
    bool parallelFlag = false;

    const int EXP_CONDITION_NUM = 1;
    const int EXP_BLOCK_NUM = 3;
    const int EXP_TRIAL_PER_BLOCK = 4;

    const int PRAC_CONDITION_NUM = 1;
    const int PRAC_BLOCK_NUM = 1;
    const int PRAC_TRIAL_PER_BLOCK = 1;

    int CONDITION_NUM = 0;
    int BLOCK_NUM = 0;
    int TRIAL_PER_BLOCK = 0;

    const int TARGET_NUM = 3;
    const float STIM_LENGTH = 2;
    const float GAZE_SHIFT_LENGTH = 1f;
    const int HIGH_FREQ_FACTOR = 3;

#if !UNITY_EDITOR
     string ip = "192.168.1.100";
     string port = "9998";
     StreamSocket socket;
#endif

    // Use this for initialization
    void Start()
    {
        if (EVENT_MARKER_TYPE == LSL_EVENT_MARKER)
        {
           info = new liblsl.StreamInfo("Unity_Event_Marker", "Markers", 1, 0, liblsl.channel_format_t.cf_string, "msi");
           outlet = new liblsl.StreamOutlet(info);
        }

        var stimulusManagerObject = GameObject.Find("Stimulus Manager");
        stimulusManager = stimulusManagerObject.GetComponent<StimulusManager>();

        var textUpdaterObject = GameObject.Find("Text Updater");
        textUpdater = textUpdaterObject.GetComponent<TextUpdater>();
        textUpdater.setText("Welcome");

        //Shuffle(conditions);

        orders = new int[TARGET_NUM];
        for (int i = 0; i < TARGET_NUM; ++i)
        {
            orders[i] = i;
        }
        Shuffle(orders);

        state = 5;
    }

    // Update is called once per frame
    void Update()
    {
        targetTime -= Time.deltaTime;

        if (targetTime <= 0.0f)
        {
            if (waitKeyState == 1)
            {
                if (setTextFlag == true)
                {
                    textUpdater.setText(s);
                    setTextFlag = false;
                }
                if (Input.anyKeyDown)
                {
                    waitKeyState = 0;
                    textUpdater.setText(" ");
                }
            }

            else if (state == 0)
            {
                ++state;
                if (EVENT_MARKER_TYPE == LPT_EVENT_MARKER)
                {
                    ParallelPortManager.Out32_x64(ParallelPortManager.address, 0);
                }

                if (trialCount == TRIAL_PER_BLOCK)
                {
                    if (conditionCount == CONDITION_NUM)
                    {
                        blockCount += 1;
                        conditionCount = 0;
                        //Shuffle(conditions);
                        Shuffle(orders);

                        if (blockCount == BLOCK_NUM)
                        {
                            Debug.Log(practiceFlag.ToString());
                            if (practiceFlag == true)
                            {
                                CONDITION_NUM = EXP_CONDITION_NUM;
                                TRIAL_PER_BLOCK = EXP_TRIAL_PER_BLOCK;
                                BLOCK_NUM = EXP_BLOCK_NUM;
                                trialCount = TRIAL_PER_BLOCK;

                                practiceFlag = false;
                                waitKeyState = 1;
                                s = "Experiment Session";
                                setTextFlag = true;
                                state = 0;   
                            }
                            else
                            {
                                if (parallelFlag == false)
                                {
                                    parallelFlag = true;
                                    state = 5;
                                }
                                else
                                {
                                    state = 8;
                                }
                            }

                            conditionCount = 0;
                            blockCount = 0;
                            stimulusManager.StopStimulating();

                            return;
                        }
                    }

                    if (parallelFlag == true)
                    {
                        setStimulus(conditions[conditionCount]);
                    }
                    

                    conditionCount += 1;
                    stimulusManager.StopStimulating();
                    trialCount = 0;
                    waitKeyState = 1;
                }

                trialCount++;

                trainingTarget = 0;
                targetTime = 0f;
            }

            else if (state == 1)
            {
                int trainingTargets = orders[trainingTarget];
                if (parallelFlag == false)
                {
                    setSerialStimulus(orders[trainingTarget], conditions[conditionCount - 1]);
                    stimulusManager.GazeShift(0);
                }
                else
                {
                    stimulusManager.GazeShift(trainingTargets);
                }

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

                targetTime = GAZE_SHIFT_LENGTH;
                state = 2;
            }
            else if (state == 2)
            {
                targetTime = STIM_LENGTH;
                state = 1;

                if (++trainingTarget == TARGET_NUM)
                {
                    state = 0;
                }

                stimulusManager.StartStimulating();

                if (practiceFlag == false)
                {
                    int eventValue = conditions[conditionCount - 1] * 10 + (orders[trainingTarget - 1] + 1);
                    //Debug.Log(eventValue.ToString());
                    if (EVENT_MARKER_TYPE == TCP_EVENT_MARKER)
                    {
                        byte[] msg = new byte[2];
                        msg[0] = (byte)(orders[trainingTarget - 1] + 1);
                        msg[1] = (byte)(conditions[conditionCount - 1] * 10);
#if !UNITY_EDITOR
            SendMessage(msg);
#endif
                    }
                    else if (EVENT_MARKER_TYPE == LPT_EVENT_MARKER)
                    {
                        //Debug.Log((conditionCount - 1).ToString());
                        //Debug.Log((trainingTarget - 1).ToString());
                        //Debug.Log("Block: " + (blockCount + 1) + ". Condition: " + conditions[conditionCount - 1] + ". Trial: " + trialCount + ". Target: " + (orders[trainingTarget - 1] + 1));
                        //int eventValue = (blockCount + 1) * 1000 + conditions[conditionCount - 1] * 100 + trialCount * 10 + (orders[trainingTarget - 1] + 1);
                        ParallelPortManager.Out32_x64(ParallelPortManager.address, eventValue);
                    }
                    else if (EVENT_MARKER_TYPE == LSL_EVENT_MARKER)
                    {
                        string[] sample = new string[1];
                        sample[0] = eventValue.ToString();
                        outlet.push_sample(sample);
                    }
                }
            }
            else if (state == 5)
            {
                waitKeyState = 1;
                s = "Practice Session";
                setTextFlag = true;
                state = 6;
            }
            else if (state == 6)
            {
                practiceFlag = true;
                CONDITION_NUM = PRAC_CONDITION_NUM;
                TRIAL_PER_BLOCK = PRAC_TRIAL_PER_BLOCK;
                BLOCK_NUM = PRAC_BLOCK_NUM;

                trialCount = TRIAL_PER_BLOCK;
                state = 0;
            }
            else if (state == 8)
            {
                waitKeyState = 1;
                s = "The experiment is over. Thank you!";
                setTextFlag = true;
                state = -100;
            }
        }
        
    }

    // condition: (1) left high, right low (2) right high, left low (3) both high (4) both low
    // (5) left 39, right 30 ~ 27 (6) left 30 ~ 27, right 39 (7) high low modulation (8) high high modulation
    void setStimulus(int condition)
    {
        Vector3[] ps = new Vector3[TARGET_NUM];
        Vector3[] qs = new Vector3[TARGET_NUM];
        float[] freqs = new float[TARGET_NUM];
        string[] labels = new string[TARGET_NUM];
        string[] layers = new string[TARGET_NUM];
        int[] modulations = Enumerable.Repeat(0, TARGET_NUM).ToArray();
        float[] modulationFreqs = Enumerable.Repeat(-1f, TARGET_NUM).ToArray();
        float[] phases = Enumerable.Repeat(0f, TARGET_NUM).ToArray();
        float[] dutyCycles = Enumerable.Repeat(0.5f, TARGET_NUM).ToArray();

        for (int i = 0; i < TARGET_NUM; i++)
        {
            ps[i] = new Vector3(9f * ((i - 1) % 2), 9f * ((i -2) % 2), z_pos);
            qs[i] = new Vector3(-90, 0, 0);
            freqs[i] = PredefinedFreqs[i];
            labels[i] = (i + 1).ToString();
            layers[i] = "Default";
        }
        stimulusManager.InitialTargets(ps, qs, freqs, labels, layers, modulations, modulationFreqs, phases, dutyCycles);
    }
    
    // condition: (1) left high, right low (2) right high, left low (3) both high (4) both low
    // (5) left 39, right 30 ~ 27 (6) left 30 ~ 27, right 39 (7) high low modulation (8) high high modulation
    void setSerialStimulus(int target, int condition)
    {
        Vector3[] ps = new Vector3[1];
        Vector3[] qs = new Vector3[1];
        float[] freqs = new float[1];
        string[] labels = new string[1];
        string[] layers = new string[1];
        int[] modulations = Enumerable.Repeat(0, 1).ToArray();
        float[] modulationFreqs = Enumerable.Repeat(-1f, 1).ToArray();
        float[] phases = Enumerable.Repeat(0f, 1).ToArray();
        float[] dutyCycles = Enumerable.Repeat(0.5f, 1).ToArray();


        for (int i = 0; i < 1; i++)
        {
            ps[i] = new Vector3(0, 0, z_pos);
            qs[i] = new Vector3(-90, 0, 0);
            freqs[i] = PredefinedFreqs[target];

            labels[i] = (target + 1).ToString();
            layers[i] = "Default";
        }
        stimulusManager.InitialTargets(ps, qs, freqs, labels, layers, modulations, modulationFreqs, phases, dutyCycles);
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

    public void Shuffle(int[] array)
    {
        System.Random random = new System.Random();
        int n = array.Length;
        while (n > 1)
        {
            n--;
            int i = random.Next(n + 1);
            int temp = array[i];
            array[i] = array[n];
            array[n] = temp;
        }
    }

}
