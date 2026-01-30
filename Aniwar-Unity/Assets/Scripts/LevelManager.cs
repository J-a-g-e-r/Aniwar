// Assets/Scripts/LevelManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

public class LevelManager : MonoBehaviour
{
    public GameObject ScrollBar;
    float scroll_pos = 0;
    float[] pos;

    [Header("Level Data References")]
    [SerializeField] private LevelData level1Data;
    [SerializeField] private LevelData level2Data;

    [Header("Loading Screen")]
    [SerializeField] private LoadingScreenUI loadingScreen;

    private CancellationTokenSource _loadingCts;

    private void Start()
    {
        // Đảm bảo LevelLoaderManager tồn tại
        if (LevelLoaderManager.Instance == null)
        {
            GameObject loaderObj = new GameObject("LevelLoaderManager");
            loaderObj.AddComponent<LevelLoaderManager>();
        }
        loadingScreen?.Hide();
    }

    private void OnDestroy()
    {
        _loadingCts?.Cancel();
        _loadingCts?.Dispose();
    }

    private void Update()
    {
        pos = new float[transform.childCount];
        float distance = 1f / (pos.Length - 1f);
        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = distance * i;
        }
        if (Input.GetMouseButton(0))
        {
            scroll_pos = ScrollBar.GetComponent<Scrollbar>().value;
        }
        else
        {
            for (int i = 0; i < pos.Length; i++)
            {
                if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2))
                {
                    ScrollBar.GetComponent<Scrollbar>().value = Mathf.Lerp(ScrollBar.GetComponent<Scrollbar>().value, pos[i], 0.1f);
                }
            }
        }

        for (int i = 0; i < pos.Length; i++)
        {
            if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2))
            {
                transform.GetChild(i).localScale = Vector2.Lerp(transform.GetChild(i).localScale, new Vector2(1f, 1f), 0.1f);
                for (int a = 0; a < pos.Length; a++)
                {
                    if (a != i)
                    {
                        transform.GetChild(a).localScale = Vector2.Lerp(transform.GetChild(a).localScale, new Vector2(0.8f, 0.8f), 0.1f);
                    }
                }
            }
        }
    }

    public async void LoadLevel1()
    {
        if (level1Data == null)
        {
            Debug.LogError("Level1Data is not assigned!");
            return;
        }

        await LoadLevelAsync(level1Data, "Level1");
    }

    public async void LoadLevel2()
    {
        if (level2Data == null)
        {
            Debug.LogError("Level2Data is not assigned!");
            return;
        }

        await LoadLevelAsync(level2Data, "Level2");
    }

    private async UniTask LoadLevelAsync(LevelData levelData, string sceneName)
    {
        // Hủy loading trước đó nếu có
        _loadingCts?.Cancel();
        _loadingCts?.Dispose();
        _loadingCts = new CancellationTokenSource();

        try
        {
            // Hiển thị loading screen
            if (loadingScreen != null)
            {
                await loadingScreen.ShowAsync();
            }

            // Preload Addressables với progress callback
            await LevelLoaderManager.Instance.PreloadLevelAssetsAsync(
                levelData,
                progress => {
                    if (loadingScreen != null)
                        loadingScreen.UpdateProgress(progress);
                },
                _loadingCts.Token
            );

            // Đảm bảo progress = 100%
            if (loadingScreen != null)
                loadingScreen.UpdateProgress(1f);

            // Delay nhỏ để user thấy 100%
            await UniTask.Delay(300, cancellationToken: _loadingCts.Token);

            // Load scene
            await SceneManager.LoadSceneAsync(sceneName).ToUniTask(cancellationToken: _loadingCts.Token);
        }
        catch (System.OperationCanceledException)
        {
            Debug.Log("Level loading cancelled");
            if (loadingScreen != null)
                await loadingScreen.HideAsync();
        }
    }
}