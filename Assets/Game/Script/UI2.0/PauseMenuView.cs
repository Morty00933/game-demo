using UnityEngine;
using UnityEngine.UI;

public class PauseMenuView : MonoBehaviour, IUIView
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button exitToMenuButton;

    public void Init(UnityEngine.Events.UnityAction onContinue, UnityEngine.Events.UnityAction onExitToMenu)
    {
        continueButton.onClick.RemoveAllListeners();
        exitToMenuButton.onClick.RemoveAllListeners();

        continueButton.onClick.AddListener(onContinue);
        exitToMenuButton.onClick.AddListener(onExitToMenu);
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}