using UnityEngine;

public class SettingsUIPanel : UIPanel
{
    private protected override void Init()
    {
        Setting[] settings = GetComponentsInChildren<Setting>(true);
        foreach (Setting setting in settings)
            setting.Load();
    }

    public void Back()
    {
        if(GameManager.IsLoaded)
            UIManager.Instance.ShowPanel(PanelType.Pause);
        else
            UIManager.Instance.HideAllPanels();
    }
}
