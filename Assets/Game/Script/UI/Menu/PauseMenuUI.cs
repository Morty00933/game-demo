using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    public static PauseMenuUI Instance { get; private set; }
    public bool GameIsPaused { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (transform.parent != null)
        {
            Debug.LogWarning("PauseMenuUI должен быть корневым объектом! Отделяем от родителя.");
            transform.SetParent(null);
        }
        DontDestroyOnLoad(gameObject);
    }

    public void PauseGame()
    {
        GameIsPaused = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        GameIsPaused = false;
        Time.timeScale = 1f;
    }
}