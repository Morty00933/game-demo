using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnPoint : MonoBehaviour
{
    [SerializeField] private bool isActive = false;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite inactiveSprite;

    private void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        UpdateVisualState();
    }

    private void OnTriggerEnter2D(Collider2D _other)
    {
        if (_other.CompareTag("Player"))
        {
            string currentScene = SceneManager.GetActiveScene().name;

            // Устанавливаем точку респавна
            GlobalController.instance.SetRespawnPoint(transform.position, currentScene);

            // Сохраняем данные
            if (SaveData.Instance != null)
                SaveData.Instance.SavePlayerData();

            isActive = true;
            UpdateVisualState();

            Debug.Log($"[RespawnPoint] Респавн установлен в {transform.position} на сцене '{currentScene}'");
        }
    }

    private void UpdateVisualState()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = isActive ? activeSprite : inactiveSprite;
        }
    }
}
