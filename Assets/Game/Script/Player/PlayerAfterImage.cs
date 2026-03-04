using System.Collections;
using UnityEngine;

public class PlayerAfterImage : MonoBehaviour
{
    [SerializeField] private float afterImageDelay = 0.1f;
    [SerializeField] private float afterImageLifetime = 0.3f;
    [SerializeField] private Color afterImageColor = new Color(1f, 1f, 1f, 0.5f);

    private float timer;
    private SpriteRenderer sr;
    private PlayerStateList pState;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        pState = GetComponent<PlayerStateList>();
        timer = afterImageDelay;
    }

    private void Update()
    {
        if (pState.currentState == PlayerStateList.State.Dashing ||
            pState.currentState == PlayerStateList.State.Parrying ||
            pState.currentState == PlayerStateList.State.Sliding)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                SpawnAfterImage();
                timer = afterImageDelay;
            }
        }
    }

    private void SpawnAfterImage()
    {
        GameObject ghost = new GameObject("AfterImage");
        ghost.transform.position = transform.position;
        ghost.transform.rotation = transform.rotation;

        SpriteRenderer ghostSR = ghost.AddComponent<SpriteRenderer>();
        ghostSR.sprite = sr.sprite;
        ghostSR.sortingLayerID = sr.sortingLayerID;
        ghostSR.sortingOrder = sr.sortingOrder - 1; // Позади игрока
        ghostSR.color = afterImageColor;

        if (transform.lossyScale.x < 0)
        {
            ghost.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            ghost.transform.localScale = Vector3.one;
        }

        StartCoroutine(FadeAndDestroy(ghostSR, ghost));
    }

    private IEnumerator FadeAndDestroy(SpriteRenderer ghostSR, GameObject ghost)
    {
        float t = 0f;
        Color startColor = ghostSR.color;

        while (t < afterImageLifetime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0, t / afterImageLifetime);
            ghostSR.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(ghost);
    }
}
