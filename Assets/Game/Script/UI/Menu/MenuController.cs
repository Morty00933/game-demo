using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MenuState
{
    MainMenu,
    InGame
}

public class MenuController : MonoBehaviour
{
    public MenuSelection Menu;

    public static MenuController instance;

    [Header("MainMenu")]
    public CanvasGroup PressAnyCanvasGroup;
    public CanvasGroup mainMenuCanvasGroup;
    public CanvasGroup CreditCanvasGroup;

    private MenuState currentMenuState;

    void Start()
    {
        currentMenuState = MenuState.MainMenu;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        switch (currentMenuState)
        {
            case MenuState.MainMenu:
                Menu?.HandleInput();
                break;

            case MenuState.InGame:
                break;
        }
    }

    public void MainMenuState()
    {
        currentMenuState = MenuState.MainMenu;
        MenuSelection.instance.FadeToMenu();
    }

    public void InGameMenu()
    {
        currentMenuState = MenuState.InGame;
    }
}