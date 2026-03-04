using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuSelection : MonoBehaviour
{
    public static MenuSelection instance;
    public RectTransform selectionBorder;
    public Button[] menuButtons;
    public CanvasGroup[] Menu;
    public GameObject leftBar, rightBar;
    public GameObject StartFirstOption, OptionMenuFirst, ClosedOptionMenu, NotificationButton, LoadGameButton;
    public GameObject NotificationPanel;
    public float moveDistance = 10f;
    public float speed = 1f;
    public int currentIndex = 0;
    private float inputDelay = 0.2f;
    private float lastInputTime;
    private Vector3 leftStartPos;
    private Vector3 rightStartPos;
    private Animator left, right;

    void Start()
    {
        left = leftBar.gameObject.GetComponent<Animator>();
        right = rightBar.gameObject.GetComponent<Animator>();
        for (int i = 0; i < menuButtons.Length; i++)
        {
            int index = i;
            EventTrigger trigger = menuButtons[i].gameObject.AddComponent<EventTrigger>();
            
            EventTrigger.Entry entryEnter = new EventTrigger.Entry();
            entryEnter.eventID = EventTriggerType.PointerEnter;
            entryEnter.callback.AddListener((data) => { OnButtonHover(index); });
            trigger.triggers.Add(entryEnter);
        }

        MoveSelectionBorder();

        leftStartPos = leftBar.transform.localPosition;
        rightStartPos = rightBar.transform.localPosition;

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

    }
    void Update()
    {
        HandleInput();
    }

    public void HandleInput()
    {
        float verticalInput = Input.GetAxis("Vertical");
        bool moved = false;
        if (Menu[0].alpha == 1f)
        {
            if (Input.anyKeyDown)
            {
                StartCoroutine(ExecuteFadeOutInSequence());
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(StartFirstOption);
            }
        }
        else
        {
            if (Menu[1].alpha > 0.99f)
            {
                if ((Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) || verticalInput < -0.5f) && Time.time - lastInputTime > inputDelay)
                {
                    currentIndex = (currentIndex + 1) % menuButtons.Length;
                    MoveSelectionBorder();
                    lastInputTime = Time.time;
                    moved = true;
                }
                else if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || verticalInput > 0.5f) && Time.time - lastInputTime > inputDelay)
                {
                    currentIndex = (currentIndex - 1 + menuButtons.Length) % menuButtons.Length;
                    MoveSelectionBorder();
                    lastInputTime = Time.time;
                    moved = true;
                }

                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0))
                {
                    StartCoroutine(Click());
                }

                if (moved)
                {
                    lastInputTime = Time.time;
                }
            }
            else if (Menu[3].alpha > 0.99f)
            {
                if (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.JoystickButton1))
                {
                    FadeToMenu();
                }
            }
        }

    }

    public void FadeOutPanel(CanvasGroup canvasGroup)
    {
        StartCoroutine(FadeOut(canvasGroup, 0.5f));
    }
    public void FadeInPanel(CanvasGroup canvasGroup)
    {
        StartCoroutine(FadeIn(canvasGroup, 0.5f));
    }

    IEnumerator Click()
    {
        StartCoroutine(MoveBars());
        left.SetBool("Click",true);
        right.SetBool("Click", true);
        yield return new WaitForSeconds(1.3f);
        menuButtons[currentIndex].onClick.Invoke();
    }

    void OnButtonHover(int index)
    {
        currentIndex = index;
        MoveSelectionBorder();
    }

    void MoveSelectionBorder()
    {
        selectionBorder.position = menuButtons[currentIndex].transform.position;
        selectionBorder.sizeDelta = menuButtons[currentIndex].GetComponent<RectTransform>().sizeDelta;
        left.SetTrigger("Move");
        right.SetTrigger("Move");
    }
    IEnumerator MoveBars()
    {
            yield return MoveBarsToPosition(moveDistance);
            yield return MoveBarsToPosition(-moveDistance);
    }

    IEnumerator MoveBarsToPosition(float targetOffset)
    {
        float elapsedTime = 0f;
        Vector3 leftTargetPos = leftStartPos + new Vector3(targetOffset, 0, 0);
        Vector3 rightTargetPos = rightStartPos + new Vector3(-targetOffset, 0, 0);

        while (elapsedTime < speed)
        {
            leftBar.transform.localPosition = Vector3.Lerp(leftBar.transform.localPosition, leftTargetPos, elapsedTime / speed);
            rightBar.transform.localPosition = Vector3.Lerp(rightBar.transform.localPosition, rightTargetPos, elapsedTime / speed);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        leftBar.transform.localPosition = leftTargetPos;
        rightBar.transform.localPosition = rightTargetPos;
    }

    IEnumerator ExecuteFadeOutInSequence()
    {
        yield return StartCoroutine(FadeOut(Menu[0], 0.5f));
        yield return StartCoroutine(FadeIn(Menu[1], 0.5f));
    }

    IEnumerator FadeOut(CanvasGroup canvasGroup, float _seconds)
    {
        if (currentIndex == 2)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(OptionMenuFirst);
        }
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 1;
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.unscaledDeltaTime / _seconds;
            yield return null;
        }
        left.SetBool("Click", false);
        right.SetBool("Click", false);
        yield return null;
    }

    IEnumerator FadeIn(CanvasGroup canvasGroup, float _seconds)
    {
        canvasGroup.alpha = 0;
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.unscaledDeltaTime / _seconds;
            yield return null;
        }
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        yield return null;
    }
    public void FadeToMenu()
    {
        if (Menu[2].alpha == 1f)
        {
            StartCoroutine(FadeOut(Menu[2], 0.5f));
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(ClosedOptionMenu);
        }
        else if (Menu[3].alpha == 1f)
        {
            StartCoroutine(FadeOut(Menu[3], 0.5f));
        }
        StartCoroutine(FadeIn(Menu[1], 0.5f));
    }

    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    public void LoadGame()
    {
        string playerDataPath = Application.persistentDataPath + "/save.player.data";
        string benchDataPath = Application.persistentDataPath + "/save.bench.data";
        if (File.Exists(playerDataPath) && File.Exists(benchDataPath))
        { 
            SaveData.Instance.LoadPlayerData(); 
        }
        else
        {
            NotificationPanel.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(NotificationButton);
        }
    }
    public void NotificationPanelOff()
    {
        NotificationPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(LoadGameButton);
    }
}
