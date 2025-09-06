using Eflatun.SceneReference;
using UnityEngine;

public class PauseUIPanel : UIPanel
{
    [SerializeField] private SceneReference menuScene;

    private protected override void Init()
    {
        
    }

    public void Resume()
    {
        if (!GameManager.IsLoaded)
            return;

        GameManager.Instance.ChangeState(GameState.Playing);
    }

    public void OpenSettings()
    {
        UIManager.Instance.ShowPanel(PanelType.Settings);
    }

    public void ReturnToMenu()
    {
        SceneLoader.Instance.FadeToScene(menuScene, () => {
            UIManager.Instance.HideAllPanels();
        });
    }
}
