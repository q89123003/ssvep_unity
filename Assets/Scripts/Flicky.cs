using HoloToolkit.Examples.InteractiveElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flicky : MonoBehaviour {

    readonly string freqPropertyName = "_FlickerFrequnecy";
    readonly string mainColorPropertyName = "_MainColor";
    readonly string blinkColorPropertyName = "_BlinkColor";
    readonly string startTimePropertyName = "_StartTime";
    readonly string phasePropertyName = "_Phase";
    readonly string modulationFrequencyPropertyName = "_ModulationFrequency";
    readonly string modulationPropertyName = "_Modulation";
    readonly string thresholdPropertyName = "_BlinkThreshold";
    readonly string amplitudePropertyName = "_Amplitude";
    //readonly string debugValuePropertyName = "_DebugValue";
    Renderer rend;

    bool isFlickering;
    //int debugRecording = 0;
    //float[] debugValues;
    //int debugValueCount = 0;

    float savedFrequency;
    float savedModulationFrequency;
    float savedPhase;
    int savedModulation;
    float savedDutyCycle;

    public void Start()
    {
        Initialize();
        //debugValues = new float[200];
    }

    public void Update()
    {
        /*
        if (debugRecording == 1)
        {
            debugValues[debugValueCount] = rend.material.GetFloat(debugValuePropertyName);
            debugValueCount += 1;
        }
        */
    }

    public void Initialize()
    {
        rend = GetComponent<Renderer>();

        rend.material.EnableKeyword(freqPropertyName);
        rend.material.EnableKeyword(mainColorPropertyName);
        rend.material.EnableKeyword(blinkColorPropertyName);
        rend.material.EnableKeyword(startTimePropertyName);
        rend.material.EnableKeyword(phasePropertyName);
        rend.material.EnableKeyword(modulationFrequencyPropertyName);
        rend.material.EnableKeyword(modulationPropertyName);
        rend.material.EnableKeyword(thresholdPropertyName);
        //rend.material.EnableKeyword(debugValuePropertyName);

        // Set frequncy to zero so no flickering at the beginning
        rend.material.SetFloat(freqPropertyName, 0);
        isFlickering = true;
    }


    public void SetFrequency(float f, int modulation = 0, float modulationFrequency = -1, float phase = 0, float dutyCycle = 0.5f)
    {
        savedFrequency = f;
        savedPhase = 0;
        savedModulation = modulation;
        savedModulationFrequency = modulationFrequency;
        //Debug.Log(f);
        if (isFlickering)
        {
            if (modulation != 0 && modulationFrequency != -1)
            {
                rend.material.SetInt(modulationPropertyName, modulation);
                rend.material.SetFloat(modulationFrequencyPropertyName, modulationFrequency);

            }

            rend.material.SetFloat(freqPropertyName, f);
            rend.material.SetFloat(phasePropertyName, phase);
            rend.material.SetFloat(thresholdPropertyName, Mathf.Cos(dutyCycle * Mathf.PI));
            rend.material.SetFloat(startTimePropertyName, Shader.GetGlobalVector("_Time").y);
        }
    }

    public void SetSavedFrequency(float f, int modulation = 0, float modulationFrequency = -1, float phase = 0, float dutyCycle = 0.5f)
    {
        savedFrequency = f;
        savedPhase = phase;
        savedModulation = modulation;
        savedModulationFrequency = modulationFrequency;
        savedDutyCycle = dutyCycle;
    }

    public void ResumeFrequency()
    {
        /*
        debugValueCount = 0;
        debugRecording = 1;
        */
        SetFrequency(savedFrequency, savedModulation, savedModulationFrequency, savedPhase, savedDutyCycle);
    }

    public void PauseFlickering()
    {
        /*
        debugRecording = 0;
        string tmp = "";
        foreach (float v in debugValues)
        {
            tmp += v + " ";
        }
        Debug.Log(tmp);
        */

        rend.material.SetFloat(freqPropertyName, 0f);
    }

    public void SetMainColor(Color c)
    {
        //Debug.Log("Set main color: " + c.ToString());
        rend.material.SetColor(mainColorPropertyName, c);
    }

    public void SetBlinkColor(Color c)
    {
        //Debug.Log("Set blink color: " + c.ToString());
        rend.material.SetColor(blinkColorPropertyName, c);
    }

}
