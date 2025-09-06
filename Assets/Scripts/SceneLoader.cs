using DG.Tweening;
using Eflatun.SceneReference;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : Singleton<SceneLoader>
{
    [SerializeField] private Image fadeImage;

    [Header("Config")]
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private Ease fadeOutEaseType = Ease.OutCirc;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private Ease fadeInEaseType = Ease.OutCirc;

    public void FadeToScene(SceneReference targetScene, Action onSceneLoadedCallback = null)
    {
        StartCoroutine(FadeSequence(targetScene, onSceneLoadedCallback));
    }

    private IEnumerator FadeSequence(SceneReference targetScene, Action onSceneLoadedCallback = null)
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = Color.clear;
        yield return fadeImage.DOFade(1f, fadeOutDuration).SetEase(fadeOutEaseType).SetUpdate(true).WaitForCompletion();
        fadeImage.color = Color.black;

        yield return SceneManager.LoadSceneAsync(targetScene.Name, LoadSceneMode.Single);
        onSceneLoadedCallback?.Invoke();

        yield return fadeImage.DOFade(0f, fadeInDuration).SetEase(fadeInEaseType).SetUpdate(true).WaitForCompletion();
        fadeImage.color = Color.clear;
        fadeImage.gameObject.SetActive(false);
    }
}
