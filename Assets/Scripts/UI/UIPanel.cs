using UnityEngine;

public abstract class UIPanel : MonoBehaviour, IInitializable
{
    void IInitializable.Initialize()
    {
        Init();
    }

    /// <summary>
    /// Called after Awake and for objects that are disabled at startup because
    /// Disabled objects dont call Awake().
    /// </summary>
    private protected abstract void Init();

    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Forces the panel to hide without any additional logic.
    /// </summary>
    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// For when the player wants to close the UI panel.
    /// This is different from Hide() because it also handles any additional cleanup or state changes needed when closing the UI.
    /// </summary>
    public abstract void CloseUI();
}
