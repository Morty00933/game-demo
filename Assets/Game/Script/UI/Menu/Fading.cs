using System.Collections;
using UnityEngine;

public class Fading : MonoBehaviour
{
    private SpriteRenderer sr;
    private float fadeTime = 3f;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public IEnumerator StartFading()
    {
        float initialFadeTime = fadeTime;

        while (fadeTime > 0)
        {
            fadeTime -= Time.deltaTime;

            if (sr.color.a > 0)
            {
                Color newColor = sr.color;
                newColor.a -= Time.deltaTime / initialFadeTime;
                sr.color = newColor;
                yield return null;
            }
        }

        gameObject.SetActive(false);
    }
}
