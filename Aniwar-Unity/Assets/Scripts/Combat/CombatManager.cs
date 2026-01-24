using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;
    public event Action<Monster> OnMonsterDied;
    public event Action<int> OnWaveCleared;
    public event Action OnAllWavesCleared;

    [Header("References")]
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private Player player;
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private StarProgressUI starProgressUI;

    private List<Monster> activeMonsters = new List<Monster>();
    private int matchChainCount = 0; // Đếm số lần match liên tiếp
    [SerializeField] private int pointPerGem = 10;
    [SerializeField] private int totalPoints = 0;
    [SerializeField] private int maxScore = 1000;


    [Header("UI References")]
    [SerializeField] private RectTransform winPanel;
    [SerializeField] private RectTransform losePanel;
    [SerializeField] private GridManager _gridManager;

    [Header("Tween Settings")]
    [SerializeField] private float tweenDuration = 0.4f;
    [SerializeField] private Ease tweenEase = Ease.OutBack;

    private Vector2 hiddenPos;
    private Vector2 showPos;


    private void Awake()
    {
        Instance = this;
        if (monsterSpawner == null)
        {
            monsterSpawner = FindObjectOfType<MonsterSpawner>();
        }
        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }

        showPos = Vector2.zero; // giữa màn hình
        hiddenPos = new Vector2(0, -Screen.height);

        winPanel.anchoredPosition = hiddenPos;
    }

    private void OnEnable()
    {
        Gem.OnGemDestroyed += HandleGemDestroyed;
        BoardStateManager.OnRefillStateCompleted += HandleRefillStateCompleted;
        BoardStateManager.OnMatchStarted += HandleMatchStarted;
        BoardStateManager.OnMatchChainEnded += HandleMatchChainEnded;
        BoardStateManager.OnNewSwapStarted += HandleNewSwapStarted;
        MonsterSpawner.OnWaveCleared += HandleWaveCleared;
        Player.OnPlayerDied += HandlePlayerDie;
        MonsterSpawner.OnAllWavesCleared += HandleAllWavesCleared;
    }

    private void OnDisable()
    {
        Gem.OnGemDestroyed -= HandleGemDestroyed;
        BoardStateManager.OnRefillStateCompleted -= HandleRefillStateCompleted;
        BoardStateManager.OnMatchStarted -= HandleMatchStarted;
        BoardStateManager.OnMatchChainEnded -= HandleMatchChainEnded;
        BoardStateManager.OnNewSwapStarted -= HandleNewSwapStarted;
        MonsterSpawner.OnWaveCleared -= HandleWaveCleared;
        Player.OnPlayerDied -= HandlePlayerDie;
        MonsterSpawner.OnAllWavesCleared -= HandleAllWavesCleared;
    }

    private void Start()
    {
        if (monsterSpawner != null)
        {
            monsterSpawner.StartWaves();
        }
        totalScoreText.text = totalPoints.ToString();
        starProgressUI.Init(maxScore);

    }

    private void HandleGemDestroyed(Gem gem)
    {
        SpawnBullet(gem);
        UIManager.Instance.ShowPointUI(pointPerGem, gem);
                if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Crack();
        }
    }

    private void SpawnBullet(Gem gem)
    {
        GameObject bullet = ObjectPooler.Instance.GetObject("Bullet", gem.transform.position, Quaternion.identity);
        bullet.GetComponent<Bullet>().Init(gem.Variant.color, gem.column);
    }

    private void HandleMonsterDied(Monster monster)
    {
        if (activeMonsters.Contains(monster))
        {
            activeMonsters.Remove(monster);
        }

        OnMonsterDied?.Invoke(monster);
    }

    private void HandleWaveCleared(int waveIndex)
    {
        OnWaveCleared?.Invoke(waveIndex);
        Debug.Log($"Wave {waveIndex + 1} cleared!");
    }

    private void HandleAllWavesCleared()
    {
        OnAllWavesCleared?.Invoke();


        float delay = 3f; // 1–2 giây
        AudioManager.Instance.StopCurrentMusicFade(delay);
        AudioManager.Instance.WinMusic();
        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);

        // Đợi
        seq.AppendInterval(delay);

        // Panel bay vào
        seq.Append(
            winPanel.DOAnchorPos(showPos, tweenDuration)
                    .SetEase(tweenEase)
        );

        // Khi panel đã vào xong
        seq.OnComplete(() =>
        {
            WinPanelUI ui = winPanel.GetComponent<WinPanelUI>();
            ui.UpdateScore(totalPoints);
            ui.ShowWinPanel(totalPoints);
            _gridManager.EnableInput(false);
        });
    }

    public void RegisterMonster(Monster monster)
    {
        if (!activeMonsters.Contains(monster))
        {
            activeMonsters.Add(monster);
        }
    }

    public List<Monster> GetActiveMonsters()
    {
        return new List<Monster>(activeMonsters);
    }

    public int GetActiveMonsterCount()
    {
        return activeMonsters.Count;
    }

    private void HandleRefillStateCompleted()
    {
        // Sau khi refill xong, tất cả monsters tấn công player
        AttackPlayer();
    }
    
    private void HandleMatchStarted()
    {
        // Tăng counter mỗi khi có match
        matchChainCount++;
        AudioManager.Instance.ComboSound(matchChainCount);
    }
    
    private void HandleMatchChainEnded()
    {
        // Phát âm thanh dựa vào số match đã đếm được trong chuỗi
        if (AudioManager.Instance != null)
        {
            if (matchChainCount == 3)
            {
                AudioManager.Instance.Exclaimations(0);
            }
            else if (matchChainCount == 4)
            {
                AudioManager.Instance.Exclaimations(1);
            }
            else if (matchChainCount == 5)
            {
                AudioManager.Instance.Exclaimations(2);
            }
            else if (matchChainCount > 5)
            {
                AudioManager.Instance.Exclaimations(3);
            }
        }
        
        UIManager.Instance.ShowExclaimation(matchChainCount);
        // Reset counter sau khi phát âm thanh
        matchChainCount = 0;
    }
    
    private void HandleNewSwapStarted()
    {
        // Reset counter khi người chơi bắt đầu swap mới (từ IdleState)
        matchChainCount = 0;
    }


    private void HandlePlayerDie()
    {

        float delay = 2f; // 1–2 giây
        AudioManager.Instance.StopCurrentMusicFade(2f);
        AudioManager.Instance.LoseMusic();

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);

        // Đợi
        seq.AppendInterval(delay);

        // Panel bay vào
        seq.Append(
            losePanel.DOAnchorPos(showPos, tweenDuration)
                    .SetEase(tweenEase)
        );

        // Khi panel đã vào xong
        seq.OnComplete(() =>
        {
            _gridManager.EnableInput(false);
        });
    }
    private void AttackPlayer()
    {
        if (player == null || player.IsDead())
        {
            return;
        }

        int totalDamage = 0;
        foreach (Monster monster in activeMonsters)
        {
            if (monster != null && !monster.IsDead())
            {
                totalDamage += monster.GetDamage();
                monster.Attack(player);
            }
        }

        if (totalDamage > 0)
        {
            player.TakeDamage(totalDamage);
            Debug.Log($"Monsters attacked player for {totalDamage} damage!");
        }
    }

    

    public void AddPoint(int points)
    {
        int oldScore = totalPoints;
        totalPoints += points;
        totalScoreText.text = totalPoints.ToString();

        starProgressUI.UpdateScore(totalPoints);
    }

    public int GetPoint() => pointPerGem;
}