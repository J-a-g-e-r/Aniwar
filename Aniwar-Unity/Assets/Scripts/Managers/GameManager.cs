using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Action IsSettingOn;

    [Header("UI References")]
    [SerializeField] private RectTransform settingPanel;
    [SerializeField] private GameObject blackOverlay;
    [SerializeField] private GridManager _gridManager;

    [Header("Tween Settings")]
    [SerializeField] private float tweenDuration = 0.4f;
    [SerializeField] private Ease tweenEase = Ease.OutBack;

    private Vector2 hiddenPos;
    private Vector2 showPos;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        showPos = Vector2.zero; // giữa màn hình
        hiddenPos = new Vector2(0, -Screen.height);

        settingPanel.anchoredPosition = hiddenPos;
        blackOverlay.SetActive(false);
    }

    // ================== SETTINGS ==================
    public void OpenSetting()
    {
        blackOverlay.SetActive(true);
        _gridManager.EnableInput(false);
        settingPanel
            .DOAnchorPos(showPos, tweenDuration)
            .SetEase(tweenEase)
            .SetUpdate(true);

        Time.timeScale = 0f; // pause game
    }

    public void CloseSetting()
    {
        settingPanel
            .DOAnchorPos(hiddenPos, tweenDuration)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                blackOverlay.SetActive(false);
            });
        _gridManager.EnableInput(true);
        Time.timeScale = 1f;
    }

    // ================== BUTTON EVENTS ==================
    public void ContinueGame()
    {
        CloseSetting();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoHome()
    {
        PlayerPrefs.SetInt("MENU_STATE", (int)MenuState.MainMenu);
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
