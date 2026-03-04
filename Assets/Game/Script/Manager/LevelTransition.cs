using UnityEngine;

public class LevelTransition : MonoBehaviour
{
    [SerializeField] private string targetScene;
    [SerializeField] private Transform spawnPointInNewScene;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Vector3 newSpawn = spawnPointInNewScene != null ? spawnPointInNewScene.position : Vector3.zero;
        GlobalController.instance.SetRespawnPoint(newSpawn, targetScene);
        GlobalController.instance.StartGame(targetScene);
    }
}
