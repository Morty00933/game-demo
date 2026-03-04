using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField][Range(1f, 10f)] private float initializationTimeout = 5f;

    private PlayerHealth playerHealth;
    private Coroutine healthAnimationCoroutine;

    private void Awake()
    {
        if (healthSlider == null)
        {
            Debug.LogError("HealthUI: Health Slider не назначен!", gameObject);
            enabled = false;
            return;
        }
        if (healthText == null)
        {
            Debug.LogError("HealthUI: Health TextMeshPro не назначен!", gameObject);
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        StartCoroutine(InitializeHealthUI());
    }

    private IEnumerator InitializeHealthUI()
    {
        float elapsed = 0f;
        while (GlobalController.instance == null || GlobalController.instance.CurrentPlayer == null)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= initializationTimeout)
            {
                Debug.LogError("HealthUI: Не удалось найти GlobalController или CurrentPlayer за время ожидания!", gameObject);
                enabled = false;
                yield break;
            }
            yield return null;
        }

        playerHealth = GlobalController.instance.CurrentPlayer.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogError("HealthUI: PlayerHealth отсутствует на объекте игрока!", gameObject);
            enabled = false;
            yield break;
        }
        if (playerHealth.Config == null)
        {
            Debug.LogError("HealthUI: Config отсутствует в PlayerHealth!", gameObject);
            enabled = false;
            yield break;
        }

        healthSlider.maxValue = playerHealth.Config.maxHealth;
        healthSlider.value = playerHealth.Health;
        UpdateHealth();
        playerHealth.onHealthChanged.AddListener(UpdateHealth);
        Debug.Log("HealthUI: Успешно инициализирован.");
    }

    public void UpdateHealth()
    {
        if (playerHealth == null || healthSlider == null || healthText == null) return;

        if (healthAnimationCoroutine != null)
            StopCoroutine(healthAnimationCoroutine);

        healthAnimationCoroutine = StartCoroutine(AnimateHealthChange(playerHealth.Health));
    }

    private IEnumerator AnimateHealthChange(float targetHealth)
    {
        float currentHealth = healthSlider.value;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            healthSlider.value = Mathf.Lerp(currentHealth, targetHealth, elapsed / duration);
            healthText.text = $"{Mathf.RoundToInt(healthSlider.value)}/{healthSlider.maxValue}";
            yield return null;
        }

        healthSlider.value = targetHealth;
        healthText.text = $"{Mathf.RoundToInt(targetHealth)}/{healthSlider.maxValue}";
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.onHealthChanged.RemoveListener(UpdateHealth);
    }

    private void OnValidate()
    {
        if (initializationTimeout < 1f) initializationTimeout = 1f;
    }
}