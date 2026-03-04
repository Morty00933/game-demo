using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Префабы UI панелей")]
    [SerializeField] private GameObject mainMenuPrefab;
    [SerializeField] private GameObject pauseMenuPrefab;
    [SerializeField] private GameObject deathMenuPrefab;

    [Header("Фейдер экрана (префаб)")]
    [SerializeField] private ScreenFader screenFaderPrefab;

    [Header("Имя игровой сцены")]
    [SerializeField] private string gameSceneName = "Location1";

    private GameObject mainMenuPanel;
    private GameObject pauseMenuPanel;
    private GameObject deathMenuPanel;

    private ScreenFader screenFader;
    private TextMeshProUGUI hudScoreText;

    private bool isPaused, isInputBlocked;
    private GameInputActions inputActions;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        inputActions = new GameInputActions();
        inputActions.UI.Pause.performed += ctx =>
        {
            if (SceneManager.GetActiveScene().name == "MainMenu") return;
            if (isInputBlocked) return;

            if (isPaused) HidePauseMenu();
            else ShowPauseMenu();
        };
        inputActions.Enable();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        inputActions?.Disable();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isPaused = false;
        isInputBlocked = false;
        Time.timeScale = 1f;
        ClearOldUI();
        InitUI(scene.name);
    }

    private void InitUI(string sceneName)
    {
        if (sceneName == "MainMenu")
        {
            mainMenuPanel = Instantiate(mainMenuPrefab);
            screenFader = Instantiate(screenFaderPrefab);
            screenFader.gameObject.SetActive(true);
            StartCoroutine(DelayedFadeIn());

            var mainMenuView = mainMenuPanel.GetComponent<MainMenuView>();
            if (mainMenuView != null)
                mainMenuView.Init(StartGame, QuitGame);
        }
        else
        {
            pauseMenuPanel = Instantiate(pauseMenuPrefab);
            deathMenuPanel = Instantiate(deathMenuPrefab);

            var deathMenuView = deathMenuPanel.GetComponent<DeathMenuView>();
            deathMenuView?.Init(() =>
            {
                HideDeathMenu();
                GlobalController.instance?.CompleteRespawn();
            });

            hudScoreText = GameObject.Find("HUDScoreText")?.GetComponent<TextMeshProUGUI>();

            var pauseView = pauseMenuPanel.GetComponent<PauseMenuView>();
            pauseView?.Init(HidePauseMenu, GoToMainMenu);
        }
    }

    private IEnumerator DelayedFadeIn()
    {
        if (screenFader == null) yield break;

        if (!screenFader.gameObject.activeSelf)
            screenFader.gameObject.SetActive(true);

        yield return new WaitUntil(() => screenFader.gameObject.activeInHierarchy);
        screenFader.FadeIn();
    }

    private void ClearOldUI()
    {
        foreach (var go in new[] { mainMenuPanel, pauseMenuPanel, deathMenuPanel })
            if (go != null) Destroy(go);

        hudScoreText = null;
    }

    public void ShowPauseMenu()
    {
        isPaused = true;
        isInputBlocked = true;
        pauseMenuPanel?.SetActive(true);
        GlobalController.instance?.BlockPlayerInput(true);
        Time.timeScale = 0f;
    }

    public void HidePauseMenu()
    {
        isPaused = false;
        isInputBlocked = false;
        pauseMenuPanel?.SetActive(false);
        GlobalController.instance?.BlockPlayerInput(false);
        Time.timeScale = 1f;
    }

    public void ShowDeathMenu()
    {
        if (deathMenuPanel == null || deathMenuPanel.activeSelf) return;

        isInputBlocked = true;
        deathMenuPanel.SetActive(true);

        var deathMenuView = deathMenuPanel.GetComponent<DeathMenuView>();

        int score = GlobalController.instance?.playerScore ?? 0;
        float time = Time.timeSinceLevelLoad;
        int enemies = GlobalController.instance?.enemiesDefeated ?? 0;

        deathMenuView.UpdateStats(score, time, enemies);
        deathMenuView.Show();

        GlobalController.instance?.BlockPlayerInput(true);
    }

    public void HideDeathMenu()
    {
        isInputBlocked = false;
        deathMenuPanel.SetActive(false);
        GlobalController.instance?.BlockPlayerInput(false);
    }

    public void GoToMainMenu()
    {
        isPaused = false;
        isInputBlocked = false;
        Time.timeScale = 1f;
        GlobalController.instance?.GoToMainMenu();
    }

    public void StartGame()
    {
        GlobalController.instance?.StartGame(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void UpdateScoreText()
    {
        if (hudScoreText == null) return;
        hudScoreText.text = $"Score: {GlobalController.instance?.playerScore ?? 0}";
    }

    public void HideAllMenus()
    {
        mainMenuPanel?.SetActive(false);
        pauseMenuPanel?.SetActive(false);
        deathMenuPanel?.SetActive(false);
    }

    public ScreenFader GetScreenFader()
    {
        return screenFader;
    }
}
