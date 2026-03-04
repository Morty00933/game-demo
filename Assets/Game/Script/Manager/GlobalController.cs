using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class GlobalController : MonoBehaviour
{
    public static GlobalController instance;

    [Header("Префаб игрока и точка спавна")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Vector3 defaultSpawnPoint;

    public GameObject CurrentPlayer { get; private set; }
    public int playerScore { get; private set; }
    public int enemiesDefeated { get; private set; }

    public event Action<GameObject> OnPlayerSpawned;

    private string respawnScene;
    private Vector3 respawnPosition;
    private bool useCustomRespawn;

    [Header("Затемнение экрана")]
    [SerializeField] private ScreenFader screenFader;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            SpawnPlayer();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(DelayedFadeIn());

        if (useCustomRespawn && scene.name == respawnScene)
        {
            SpawnPlayer(respawnPosition);
            useCustomRespawn = false;
        }
    }

    private IEnumerator DelayedFadeIn()
    {
        if (screenFader == null)
            yield break;

        if (!screenFader.gameObject.activeSelf)
            screenFader.gameObject.SetActive(true);

        yield return new WaitUntil(() => screenFader.gameObject.activeInHierarchy);
        screenFader.FadeIn();
    }

    public void StartGame(string gameScene)
    {
        StartCoroutine(TransitionToScene(gameScene));
    }

    public void GoToMainMenu()
    {
        StartCoroutine(TransitionToScene("MainMenu"));
    }

    public void SpawnPlayer()
    {
        Vector3 spawnPos = GetSpawnPoint();
        SpawnPlayer(spawnPos);
    }

    public void SpawnPlayer(Vector3 position)
    {
        if (CurrentPlayer != null)
            Destroy(CurrentPlayer);

        CurrentPlayer = Instantiate(playerPrefab, position, Quaternion.identity);
        OnPlayerSpawned?.Invoke(CurrentPlayer);
    }

    public void CompleteRespawn()
    {
        if (!string.IsNullOrEmpty(respawnScene) && respawnScene != SceneManager.GetActiveScene().name)
        {
            StartCoroutine(TransitionToScene(respawnScene, respawnPosition));
        }
        else
        {
            SpawnPlayer(respawnPosition != Vector3.zero ? respawnPosition : GetSpawnPoint());
        }
    }

    public void SetRespawnPoint(Vector3 position, string sceneName)
    {
        respawnPosition = position;
        respawnScene = sceneName;
        useCustomRespawn = true;
    }

    public Vector3 RespawnPoint => respawnPosition;

    public void AddScore(int amount)
    {
        playerScore += amount;
        UIManager.Instance?.UpdateScoreText();
    }

    public void SetPlayerScore(int value)
    {
        playerScore = value;
        UIManager.Instance?.UpdateScoreText();
    }

    public void AddEnemyKill()
    {
        enemiesDefeated++;
    }

    public void BlockPlayerInput(bool block)
    {
        if (CurrentPlayer != null)
        {
            var controller = CurrentPlayer.GetComponent<PlayerController>();
            if (controller != null)
                controller.InputEnable = !block;
        }
    }

    private Vector3 GetSpawnPoint()
    {
        GameObject spawn = GameObject.FindGameObjectWithTag("SpawnPoint");
        return spawn != null ? spawn.transform.position : defaultSpawnPoint;
    }

    private IEnumerator TransitionToScene(string sceneName, Vector3? customSpawn = null)
    {
        if (screenFader != null)
        {
            if (!screenFader.gameObject.activeSelf)
                screenFader.gameObject.SetActive(true);

            yield return new WaitUntil(() => screenFader.gameObject.activeInHierarchy);

            bool fadeFinished = false;
            screenFader.FadeOut(() => fadeFinished = true);
            yield return new WaitUntil(() => fadeFinished);
        }

        if (customSpawn.HasValue)
        {
            SetRespawnPoint(customSpawn.Value, sceneName);
        }

        SceneManager.LoadScene(sceneName);
    }
}
