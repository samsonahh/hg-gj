using UnityEngine;

public abstract class Setting : MonoBehaviour
{
    /// <summary>
    /// The key used to store the setting in PlayerPrefs.
    /// </summary>
    private protected abstract string saveKey { get; }

    /// <summary>
    /// Equivalent to Awake, but called manually to ensure settings are initialized even if the object is disabled at startup.
    /// Grab the current value from PlayerPrefs and apply it to the setting.
    /// </summary>
    public abstract void Load();

    /// <summary>
    /// Save the current value of the setting to PlayerPrefs and apply it to the game.
    /// </summary>
    public abstract void Apply();
}
