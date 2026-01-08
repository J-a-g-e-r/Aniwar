using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;
    public event Action<Monster> OnMonsterDied;
    public event Action<int> OnWaveCleared;
    public event Action OnAllWavesCleared;

    [Header("References")]
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private Player player;

    private List<Monster> activeMonsters = new List<Monster>();

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
    }

    private void OnEnable()
    {
        Gem.OnGemDestroyed += HandleGemDestroyed;
        BoardStateManager.OnRefillStateCompleted += HandleRefillStateCompleted;
        MonsterSpawner.OnWaveCleared += HandleWaveCleared;
        MonsterSpawner.OnAllWavesCleared += HandleAllWavesCleared;
    }

    private void OnDisable()
    {
        Gem.OnGemDestroyed -= HandleGemDestroyed;
        BoardStateManager.OnRefillStateCompleted -= HandleRefillStateCompleted;
        MonsterSpawner.OnWaveCleared -= HandleWaveCleared;
        MonsterSpawner.OnAllWavesCleared -= HandleAllWavesCleared;
    }

    private void Start()
    {
        if (monsterSpawner != null)
        {
            monsterSpawner.StartWaves();
        }
    }

    private void HandleGemDestroyed(Gem gem)
    {
        SpawnBullet(gem);
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
        Debug.Log("Chiến thắng!");
        // TODO: Trigger win condition
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
            }
        }

        if (totalDamage > 0)
        {
            player.TakeDamage(totalDamage);
            Debug.Log($"Monsters attacked player for {totalDamage} damage!");
        }
    }
}