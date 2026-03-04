using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI fpsCounterText;
    [SerializeField][Range(0.1f, 5f)] private float updateInterval = 1f; // Настраиваемый интервал обновления

    private float fps;
    private float accumulationTime;
    private int frameCount;

    private void Awake()
    {
        if (fpsCounterText == null)
        {
            Debug.LogError("FPSCounter: FPS TextMeshPro не назначен!", gameObject);
            enabled = false;
            return;
        }
        fpsCounterText.text = "FPS: 0"; // Инициализация текста
    }

    private void Update()
    {
        accumulationTime += Time.unscaledDeltaTime;
        frameCount++;

        if (accumulationTime >= updateInterval)
        {
            fps = frameCount / accumulationTime;
            UpdateFPSText();
            accumulationTime = 0f;
            frameCount = 0;
        }
    }

    private void UpdateFPSText()
    {
        if (fpsCounterText != null)
        {
            fpsCounterText.text = $"FPS: {(int)fps}";
            // Пример: изменение цвета текста
            if (fps < 30) fpsCounterText.color = Color.red;
            else if (fps < 60) fpsCounterText.color = Color.yellow;
            else fpsCounterText.color = Color.green;
        }
    }

    private void OnDisable()
    {
        if (fpsCounterText != null)
            fpsCounterText.text = "FPS: 0"; // Сброс текста при отключении
    }
}