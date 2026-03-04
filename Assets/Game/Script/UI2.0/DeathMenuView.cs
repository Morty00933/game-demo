using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class DeathMenuView : MonoBehaviour, IUIView
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI enemiesText;
    [SerializeField] private TextMeshProUGUI highScoresText;

    private const string HIGH_SCORES_KEY = "HighScores";
    private const int MAX_HIGH_SCORES = 5;
    private List<int> highScores = new();

    private System.Action onClickRespawn;

    public void Init(System.Action onRespawn)
    {
        onClickRespawn = onRespawn;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    public void UpdateStats(int score, float time, int enemies)
    {
        scoreText.text = $"Score: {score}";
        timeText.text = $"Time: {time:00.00}";
        enemiesText.text = $"Enemies Defeated: {enemies}";

        highScores.Add(score);
        highScores = highScores.OrderByDescending(s => s).Take(MAX_HIGH_SCORES).ToList();
        PlayerPrefs.SetString(HIGH_SCORES_KEY, string.Join(",", highScores));
        PlayerPrefs.Save();

        highScoresText.text = "Top Scores:\n" +
            string.Join("\n", highScores.Select((s, i) => $"{i + 1}. {s} pts"));
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // клик мышью в любое место
        {
            // Вместо вызова Respawn → перезапускаем текущую сцену
            UIManager.Instance?.FadeToScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
