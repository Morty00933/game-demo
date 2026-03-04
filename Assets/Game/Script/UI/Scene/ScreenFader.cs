using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    [Header("Настройки затухания")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1.5f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            Debug.LogError("ScreenFader: CanvasGroup не найден!", this);

        if (fadeImage == null)
        {
            fadeImage = GetComponentInChildren<Image>();
            if (fadeImage == null)
                Debug.LogError("ScreenFader: fadeImage не найден среди детей!", this);
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void FadeOut(Action onComplete = null)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        StartCoroutine(FadeRoutine(0f, 1f, onComplete));
    }

    public void FadeIn(Action onComplete = null)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        StartCoroutine(FadeRoutine(1f, 0f, onComplete));
    }

    private IEnumerator FadeRoutine(float fromAlpha, float toAlpha, Action onComplete)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;
            t = t * t * (3f - 2f * t); // плавная интерполяция
            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = toAlpha;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        onComplete?.Invoke();
    }
}
