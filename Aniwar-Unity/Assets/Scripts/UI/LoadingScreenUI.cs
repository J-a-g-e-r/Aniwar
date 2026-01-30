using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

public class LoadingScreenUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Settings")]
    [SerializeField]
    private string[] loadingMessages = {
        "Loading gems...",
        "Preparing level...",
        "Almost there..."
    };

    private void Awake()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    public void Show()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        UpdateProgress(0f);
    }

    public void Hide()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    public void UpdateProgress(float progress)
    {
        if (progressBar != null)
            progressBar.value = progress;

        if (progressText != null)
            progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";

        // Ð?i loading message d?a trên progress
        if (loadingText != null && loadingMessages.Length > 0)
        {
            int messageIndex = Mathf.FloorToInt(progress * loadingMessages.Length);
            messageIndex = Mathf.Clamp(messageIndex, 0, loadingMessages.Length - 1);
            loadingText.text = loadingMessages[messageIndex];
        }
    }

    /// <summary>
    /// Hi?n th? loading v?i animation fade in/out
    /// </summary>
    public async UniTask ShowAsync()
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            var canvasGroup = loadingPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                await UniTask.Delay(100); // Delay nh? ð? ð?m b?o object active
                // Fade in animation có th? thêm ? ðây n?u mu?n
            }
        }
        UpdateProgress(0f);
    }

    public async UniTask HideAsync()
    {
        if (loadingPanel != null)
        {
            var canvasGroup = loadingPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                // Fade out animation có th? thêm ? ðây
                await UniTask.Delay(200);
            }
            loadingPanel.SetActive(false);
        }
    }
}