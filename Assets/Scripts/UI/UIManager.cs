using AYellowpaper.SerializedCollections;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum PanelType
{
    Pause,
    Settings,
    Results,
}

public class UIManager : Singleton<UIManager>
{
    [Header("Panels")]
    [SerializeField, SerializedDictionary("Panel Type", "Panel")]
    private SerializedDictionary<PanelType, UIPanel> panels = new SerializedDictionary<PanelType, UIPanel>();
    [field: SerializeField, ReadOnly] public UIPanel CurrentPanel { get; private set; }

    /// <summary>
    /// Action that is invoked when the current panel is changed.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description><c>UIPanel newPanel</c>: The new panel that was switched to.</description></item>
    /// </list>
    /// </remarks>
    public event Action<UIPanel> OnPanelChanged = delegate { };
    /// <summary>
    /// Action that is invoked when UI under the UIManager is closed.
    /// </summary>
    public event Action OnUIClose = delegate { };

    /// <summary>
    /// Shows the specified panel, hiding the current one if it exists.
    /// </summary>
    public void ShowPanel(PanelType panelName)
    {
        UIPanel panel = panels[panelName];

        if (CurrentPanel == panel)
            return;

        if (CurrentPanel != null && CurrentPanel != panel)
            CurrentPanel.Hide();

        CurrentPanel = panel;
        CurrentPanel.Show();

        OnPanelChanged.Invoke(CurrentPanel);

        InputManager.Instance.EnableUIActions();
    }

    /// <summary>
    /// Essentially closes the UI by hiding all panels and resetting the current panel.
    /// </summary>
    public void HideAllPanels()
    {
        foreach (UIPanel panel in panels.Values)
            panel.Hide();

        CurrentPanel = null;

        OnUIClose.Invoke();
    }
}
