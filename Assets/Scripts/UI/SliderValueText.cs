using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderValueText : MonoBehaviour
{
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private Slider slider;

    private void Start()
    {
        Slider_OnValueChanged(slider.value);
    }

    private void OnEnable()
    {
        slider.onValueChanged.AddListener(Slider_OnValueChanged);
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(Slider_OnValueChanged);
    }

    private void Slider_OnValueChanged(float newValue)
    {
        string displayString = newValue.ToString();
        if (!slider.wholeNumbers)
            displayString = Utils.FloatToString(newValue, 2);

        displayText.text = $"{displayString}";
    }
}