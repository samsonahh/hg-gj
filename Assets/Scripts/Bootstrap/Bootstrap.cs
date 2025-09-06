using UnityEngine;

public class Bootstrap : Singleton<Bootstrap>
{
    private protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);
    }
}

public static class BootstrapLoader
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadBootstrap()
    {
        if (GameObject.FindFirstObjectByType<Bootstrap>() == null)
        {
            GameObject prefab = Resources.Load<GameObject>("Bootstrap");
            GameObject go = GameObject.Instantiate(prefab);
        }
    }
}