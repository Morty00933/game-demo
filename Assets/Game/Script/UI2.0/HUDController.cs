using UnityEngine;
using UnityEngine.SceneManagement;

public class HUDController : MonoBehaviour
{
    [SerializeField] private GameObject playerIcon;
    [SerializeField] private HealthUI healthUI;
    [SerializeField] private FPSCounter fpsCounter;

    void Awake()
    {
        if (playerIcon == null)
        {
            Debug.LogError("HUDController: Player Icon не назначен!", gameObject);
            enabled = false;
            return;
        }
        if (healthUI == null)
        {
            Debug.LogError("HUDController: HealthUI не назначен!", gameObject);
            enabled = false;
            return;
        }
        if (fpsCounter == null)
        {
            Debug.LogError("HUDController: FPSCounter не назначен!", gameObject);
            enabled = false;
            return;
        }
    }

    void Start()
    {
        CheckScene();
    }

    public void CheckScene()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu" || SceneManager.GetActiveScene().name == "TutorialBoard Map")
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
}