using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StimulusManager : MonoBehaviour {

    public GameObject flicky;
    public GameObject labelText;

    List<GameObject> targetList = null;
    List<Flicky> flickyList = null;
    List<GameObject> labelTextList = null;

    private void Start()
    {
        Application.targetFrameRate = 300;
    }

    private void Update()
    {

    }

    public void InitialTargets(Vector3[] positions, Vector3[] qs, float[] freqs, string[] labels = null, string[] layers = null,
            int[] modulations = null, float[] modulationFreqs = null, float[] phases = null, float[] dutyCycles = null)
    {
        if (targetList != null)
        {
            foreach (GameObject gameObject in targetList)
            {
                Destroy(gameObject);
            }

            foreach (GameObject gameObject in labelTextList)
            {
                Destroy(gameObject);
            }
        }
        if (labels == null)
        {
            labels = new string[positions.Length];

            for (int i = 0; i < positions.Length; ++i)
            {
                labels[i] = i.ToString();
            }
        }
        if (layers == null)
        {
            layers = new string[positions.Length];

            for (int i = 0; i < positions.Length; ++i)
            {
                layers[i] = "Default";
            }
        }
        if (modulations == null)
        {
            modulations = Enumerable.Repeat(0, positions.Length).ToArray();
        }
        if (modulationFreqs == null)
        {
            modulationFreqs = Enumerable.Repeat(-1f, positions.Length).ToArray();
        }
        if (phases == null)
        {
            phases = Enumerable.Repeat(0f, positions.Length).ToArray();
        }

        if (dutyCycles == null)
        {
            dutyCycles = Enumerable.Repeat(0.5f, positions.Length).ToArray();
        }

        targetList = new List<GameObject>();
        flickyList = new List<Flicky>();
        labelTextList = new List<GameObject>();
        Color Gray = new Color(0.3f, 0.3f, 0.3f, 1);

        for (int i = 0; i < positions.Length; ++i)
        {
            GameObject tmpObject = Instantiate(flicky, positions[i], Quaternion.Euler(qs[i]));
            Flicky tmpFlicky = tmpObject.GetComponent<Flicky>();

            tmpObject.layer = LayerMask.NameToLayer(layers[i]);
            tmpFlicky.Initialize();
            tmpFlicky.SetSavedFrequency(freqs[i], modulations[i], modulationFreqs[i], phases[i], dutyCycles[i]);
            tmpFlicky.SetMainColor(Gray);
            targetList.Add(tmpObject);
            flickyList.Add(tmpFlicky);

            GameObject tmpLabelObject = Instantiate(labelText, positions[i], Quaternion.Euler(0, 0, 0));
            TextMesh tmpTextMesh = tmpLabelObject.GetComponent<TextMesh>();
            tmpTextMesh.text = labels[i];
            labelTextList.Add(tmpLabelObject);
        }
    }

    public void InitialTargets(int cubePerColumn, int cubePerRow, float startX, float startY, float startFreq, float freqResolution)
    {
        Vector3[] positions = new Vector3[cubePerColumn * cubePerRow];
        float[] freqs = new float[cubePerColumn * cubePerRow];
        Vector3[] qs = new Vector3[cubePerColumn * cubePerRow];
        string[] labels = new string[cubePerColumn * cubePerRow];

        for (int i = 0; i < cubePerColumn; ++i)
        {
            float y_pos = startY + i * -15f;
            for (int j = 0; j < cubePerRow; ++j)
            {
                int index = (i * cubePerRow + j);
                float x_pos = startX + j * 15f;
                positions[index] = new Vector3(x_pos, y_pos, 100);
                qs[index] = new Vector3(-90, 0, 0);
                freqs[index] = startFreq + index * freqResolution;
                labels[index] = index.ToString();
            }
        }

        InitialTargets(positions, qs, freqs, labels);
    }

    public void GazeShift(int target)
    {
        StopStimulating();
        Color Gray = new Color(0.3f, 0.3f, 0.3f, 1);

        foreach (Flicky f in flickyList)
        {
            f.SetMainColor(Gray);
        }

        Color Red = new Color(1, 0.3f, 0.3f, 1);
        Flicky flicky = flickyList[target];

        flicky.SetMainColor(Red);
    }

    public void GazeShift(int[] targets)
    {
        StopStimulating();
        Color Gray = new Color(0.3f, 0.3f, 0.3f, 1);

        foreach (Flicky f in flickyList)
        {
            f.SetMainColor(Gray);
        }

        Color Red = new Color(1, 0.3f, 0.3f, 1);

        foreach (int target in targets)
        {
            Flicky flicky = flickyList[target];
            flicky.SetMainColor(Red);
        }
        
    }

    public void GazeShift()
    {
        StopStimulating();
        Color Gray = new Color(0.3f, 0.3f, 0.3f, 1);

        foreach (Flicky f in flickyList)
        {
            f.SetMainColor(Gray);
        }
    }

        public void StartStimulating()
    {
        Color White = new Color(1, 1, 1, 1);

        foreach(Flicky flicky in flickyList)
        {
            flicky.SetMainColor(White);
            flicky.ResumeFrequency();
        }
    }

    public void StopStimulating()
    {
        if (flickyList == null)
            return;
        Color Black = new Color(0, 0, 0, 1);
        foreach (Flicky flicky in flickyList)
        {
            flicky.SetMainColor(Black);
            flicky.PauseFlickering();
        }
    }

    //Debug.Log(Time.deltaTime.ToString());
}
