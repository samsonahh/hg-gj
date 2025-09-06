using Eflatun.SceneReference;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private SceneReference mainScene;

    public void StartGame()
    {
        SceneLoader.Instance.FadeToScene(mainScene);
    }

    public void OpenSettings()
    {
        UIManager.Instance.ShowPanel(PanelType.Settings);
    }
}
