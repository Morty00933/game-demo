using System.Collections;
using UnityEngine;

public class FadeUI : MonoBehaviour
{
    public enum FadeDirection { In, Out }

    public virtual void FadeUIIn(CanvasGroup canvasGroup, float seconds)
    {
        StartCoroutine(Fade(canvasGroup, FadeDirection.In, seconds));
    }

    public virtual void FadeUIOut(CanvasGroup canvasGroup, float seconds)
    {
        StartCoroutine(Fade(canvasGroup, FadeDirection.Out, seconds));
    }

    private IEnumerator Fade(CanvasGroup canvasGroup, FadeDirection fadeDirection, float fadeTime)
    {
        if (fadeTime <= 0f) fadeTime = 0.01f;

        float startAlpha = fadeDirection == FadeDirection.In ? 0f : 1f;
        float endAlpha   = fadeDirection == FadeDirection.In ? 1f : 0f;
        float sign       = fadeDirection == FadeDirection.In ? 1f : -1f;

        if (fadeDirection == FadeDirection.In)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        while ((fadeDirection == FadeDirection.Out && startAlpha > endAlpha) ||
               (fadeDirection == FadeDirection.In  && startAlpha < endAlpha))
        {
            canvasGroup.alpha = startAlpha;
            startAlpha += sign * (Time.unscaledDeltaTime / fadeTime);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;

        if (fadeDirection == FadeDirection.Out)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
