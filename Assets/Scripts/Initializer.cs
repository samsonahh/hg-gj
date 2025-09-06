using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Initializer : Singleton<Initializer>
{
    private HashSet<IInitializable> initializedObjects = new();

    private protected override void Awake()
    {
        base.Awake();

        SceneManager.activeSceneChanged += SceneManager_ActiveSceneChanged;
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= SceneManager_ActiveSceneChanged;
    }

    private void SceneManager_ActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        MonoBehaviour[] sceneObjects = GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (sceneObjects == null)
            return;

        if(sceneObjects.Length == 0)
            return;

        foreach(MonoBehaviour obj in sceneObjects)
        {
            IInitializable initializable = obj.GetComponent<IInitializable>();
            if (initializable == null)
                continue;

            if(initializedObjects.Contains(initializable))
                continue;

            initializable.Initialize();
            initializedObjects.Add(initializable);
        }
    }
}
