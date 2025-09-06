using UnityEngine;
using UnityEngine.UI;

public class MasterVolumeSetting : Setting
{
    private protected override string saveKey => "MasterVolume";
    private static readonly float defaultValue = 1f;
    public static float CurrentValue { get; private set; }

    [SerializeField] private Slider volumeSlider;

    private void OnEnable()
    {
        volumeSlider.value = CurrentValue;
    }

    public override void Load()
    {
        CurrentValue = PlayerPrefs.GetFloat(saveKey, defaultValue);
        volumeSlider.value = CurrentValue;
        AudioManager.Instance.SetMasterVolume(CurrentValue);

        volumeSlider.onValueChanged.AddListener((value) => {
            Apply();
        });
    }

    public override void Apply()
    {
        float volume = volumeSlider.value;
        PlayerPrefs.SetFloat(saveKey, volume);
        CurrentValue = volume;

        AudioManager.Instance.SetMasterVolume(volume);
    }
}