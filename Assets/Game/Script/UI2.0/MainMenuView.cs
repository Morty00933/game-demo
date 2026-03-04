using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class MainMenuView : MonoBehaviour, IUIView
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;

    public void Init(UnityAction onStart, UnityAction onExit)
    {
        startButton.onClick.RemoveAllListeners();
        exitButton.onClick.RemoveAllListeners();

        startButton.onClick.AddListener(onStart);
        exitButton.onClick.AddListener(onExit);
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}