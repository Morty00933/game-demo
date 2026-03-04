using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class PlayerSaveData
{
    public Vector2 position;
    public string benchSceneName;
    public int playerScore;
}

public class SaveData : MonoBehaviour
{
    public static SaveData Instance { get; private set; }

    [Header("Сцены, доступные для загрузки")]
    public List<string> sceneNames = new List<string>();

    private string playerDataPath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        playerDataPath = Path.Combine(Application.persistentDataPath, "player_save.json");
    }

    public void SavePlayerData()
    {
        var data = new PlayerSaveData
        {
            position = GlobalController.instance.RespawnPoint,
            benchSceneName = SceneManager.GetActiveScene().name,
            playerScore = GlobalController.instance.playerScore
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(playerDataPath, json);

        Debug.Log("Player data saved to: " + playerDataPath);
    }

    public void LoadPlayerData()
    {
        if (!File.Exists(playerDataPath))
        {
            Debug.LogWarning("No save file found!");
            return;
        }

        string json = File.ReadAllText(playerDataPath);
        var data = JsonUtility.FromJson<PlayerSaveData>(json);

        if (!string.IsNullOrEmpty(data.benchSceneName) && data.benchSceneName != SceneManager.GetActiveScene().name)
        {
            GlobalController.instance.SetRespawnPoint(data.position, data.benchSceneName);
            GlobalController.instance.StartGame(data.benchSceneName);
        }
        else
        {
            GlobalController.instance.SetRespawnPoint(data.position, data.benchSceneName);
            GlobalController.instance.CompleteRespawn();
        }

        GlobalController.instance.SetPlayerScore(data.playerScore);
        Debug.Log("Player data loaded from: " + playerDataPath);
    }

    public void DeleteSaveData()
    {
        if (File.Exists(playerDataPath))
        {
            File.Delete(playerDataPath);
            Debug.Log("Save data deleted");
        }
    }
}
