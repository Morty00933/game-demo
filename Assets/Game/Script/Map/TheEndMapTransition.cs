using UnityEngine;
using UnityEngine.SceneManagement;

public class TheEndMapTransition : MonoBehaviour
{
    public static TheEndMapTransition Instance { get; private set; }
    [SerializeField] private string sceneToLoad;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(EndGameTransition());
        }
    }

    private System.Collections.IEnumerator EndGameTransition()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(sceneToLoad);
        if (GlobalController.instance?.CurrentPlayer != null)
            GlobalController.instance.CurrentPlayer.transform.position = Vector2.zero;
    }
}