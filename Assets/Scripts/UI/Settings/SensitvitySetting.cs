using UnityEngine;
using UnityEngine.UI;

public class SensitvitySetting : Setting
{
    private protected override string saveKey => "Sensitivity";
    private static readonly int defaultValue = 50;
    public static float CurrentValue { get; private set; }

    [SerializeField] private Slider sensitivitySlider;

    private void OnEnable()
    {
        sensitivitySlider.value = CurrentValue;
    }

    public override void Load()
    {
        CurrentValue = PlayerPrefs.GetInt(saveKey, defaultValue);
        sensitivitySlider.value = CurrentValue;

        sensitivitySlider.onValueChanged.AddListener((value) => {
            Apply();
        });
    }

    public override void Apply()
    {
        int sensitivity = (int)sensitivitySlider.value;
        PlayerPrefs.SetInt(saveKey, sensitivity);
        CurrentValue = sensitivity;
    }
}