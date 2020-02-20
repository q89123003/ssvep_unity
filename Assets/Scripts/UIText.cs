using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIText : MonoBehaviour {

    private Text ValueText;

    private void Start()
    {
        ValueText = GetComponent<Text>();
    }

    public void OnSliderValueChanged(float value)
    {
        ValueText.text = value.ToString("0.00");
    }
}
