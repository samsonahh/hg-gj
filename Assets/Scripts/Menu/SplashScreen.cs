using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SplashScreen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image splashImage;

    [Header("Config")]
    [SerializeField] private bool skipSplash = false;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private Ease fadeInEaseType = Ease.Linear;
    [SerializeField] private float showLogoDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private Ease fadeOutEaseType = Ease.Linear;
    [SerializeField] private float backgroundFadeDuration = 0.5f;
    [SerializeField] private Ease backgroundFadeEaseType = Ease.Linear;

    private void Start()
    {
        StartCoroutine(PlaySplashScreenCoroutine());
    }

    private IEnumerator PlaySplashScreenCoroutine()
    {
        if (skipSplash)
        {
            gameObject.SetActive(false);
            yield break;
        }

        backgroundImage.gameObject.SetActive(true);

        splashImage.color = new Color(1f, 1f, 1f, 0f); // clear white
        yield return splashImage.DOFade(1f, fadeInDuration).SetEase(fadeInEaseType).WaitForCompletion();
        splashImage.color = Color.white;

        yield return new WaitForSeconds(showLogoDuration);

        yield return splashImage.DOFade(0f, fadeOutDuration).SetEase(fadeOutEaseType).WaitForCompletion();
        splashImage.color = new Color(1f, 1f, 1f, 0f); // clear white

        yield return backgroundImage.DOFade(0f, backgroundFadeDuration).SetEase(backgroundFadeEaseType).WaitForCompletion();
        gameObject.SetActive(false);
    }
}
