using Eflatun.SceneReference;
using UnityEngine;

public class ResultsUIPanel : UIPanel
{
    [SerializeField] private SceneReference menuScene;

    private protected override void Init()
    {
        
    }

    public void Replay()
    {
        SceneLoader.Instance.FadeToScene(Utils.GetCurrentScene());
    }

    public void ReturnToMenu()
    {
        SceneLoader.Instance.FadeToScene(menuScene, () => {
            UIManager.Instance.HideAllPanels();
        });
    }
}
