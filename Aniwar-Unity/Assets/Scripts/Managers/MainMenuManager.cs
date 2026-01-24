using UnityEngine;
using DG.Tweening;

public enum MenuState
{
    MainMenu,
    SelectLevel
}

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;

    [Header("Main Menu")]
    public RectTransform title;
    public RectTransform playButton;

    [Header("Select Level")]
    public RectTransform selectLevelPanel;

    [Header("Setting")]
    public float moveDuration = 0.5f;
    public Ease ease = Ease.InOutBack;

    private Vector2 titleStartPos;
    private Vector2 playStartPos;

    private const string MENU_STATE_KEY = "MENU_STATE";

    void Awake()
    {
        Instance = this;

    }

    void Start()
    {
        titleStartPos = title.anchoredPosition;
        playStartPos = playButton.anchoredPosition;

        MenuState state = (MenuState)PlayerPrefs.GetInt(MENU_STATE_KEY, 0);

        if (state == MenuState.MainMenu)
        {
            SetupMainMenuInstant();
        }
        else
        {
            SetupSelectLevelInstant();
        }
    }

    // ================= PLAY =================
    public void OnPlayButtonClick()
    {
        PlayerPrefs.SetInt(MENU_STATE_KEY, (int)MenuState.SelectLevel);

        title.DOAnchorPosX(-Screen.width - 1200, moveDuration).SetEase(ease);
        playButton.DOAnchorPosX(-Screen.width - 1200, moveDuration).SetEase(ease);

        selectLevelPanel
            .DOAnchorPos(Vector2.zero, moveDuration)
            .SetEase(ease)
            .SetDelay(0.1f);
    }

    // ================= SETUP KHÔNG ANIMATION =================
    void SetupMainMenuInstant()
    {
        title.anchoredPosition = titleStartPos;
        playButton.anchoredPosition = playStartPos;
        selectLevelPanel.anchoredPosition = new Vector2(Screen.width + 1200, 0);
    }

    void SetupSelectLevelInstant()
    {
        title.anchoredPosition = new Vector2(-Screen.width - 1200, titleStartPos.y);
        playButton.anchoredPosition = new Vector2(-Screen.width - 1200, playStartPos.y);
        selectLevelPanel.anchoredPosition = Vector2.zero;
    }
}

