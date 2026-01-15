using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MonsterSpawnData
{
    public int monsterPrefabIndex = 0; // Index trong monsterPrefabs array
    public int hp = 100;
    public int damage = 10;
    public int columnWidth = 2;
    public int spawnColumn = -1; // -1 để tự động random
}

[System.Serializable]
public class WaveData
{
    public List<MonsterSpawnData> monsters = new List<MonsterSpawnData>();
}

public class MonsterSpawner : MonoBehaviour
{
    public static event Action<int> OnWaveCleared;
    public static event Action OnAllWavesCleared;

    [Header("Wave Settings")]
    [SerializeField] private List<WaveData> waves = new List<WaveData>();
    [SerializeField] private float spawnOffsetY = 1.5f; // Offset ph�a tr�n board

    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameObject[] monsterPrefabs; // Eliza, Shinjou prefabs

    private int currentWaveIndex = 0;
    private List<Monster> activeMonsters = new List<Monster>();
    private bool waitingForIdleState = false;
    private int pendingWaveIndex = -1;

    private void Awake()
    {
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();

        // Subscribe to monster death events
        Monster.OnMonsterDied += HandleMonsterDied;
        
        // Subscribe to idle state events
        BoardStateManager.OnIdleStateEntered += OnIdleStateEntered;
    }

    private void OnDestroy()
    {
        Monster.OnMonsterDied -= HandleMonsterDied;
        BoardStateManager.OnIdleStateEntered -= OnIdleStateEntered;
    }
    
    private void OnIdleStateEntered()
    {
        // If we're waiting for idle state to spawn next wave, spawn it now
        if (waitingForIdleState && pendingWaveIndex >= 0)
        {
            waitingForIdleState = false;
            int waveToSpawn = pendingWaveIndex;
            pendingWaveIndex = -1;
            SpawnWave(waveToSpawn);
        }
    }

    public void StartWaves()
    {
        currentWaveIndex = 0;
        if (waves.Count > 0)
        {
            SpawnWave(0);
        }
    }

    public void SpawnWave(int waveIndex)
    {
        if (waveIndex < 0 || waveIndex >= waves.Count)
        {
            Debug.LogWarning($"Wave index {waveIndex} is out of range");
            return;
        }

        currentWaveIndex = waveIndex;

        WaveData wave = waves[waveIndex];
        UIManager.Instance.ShowWaveUI(waveIndex + 1,waves.Count);
        // Generate spawn columns for monsters that don't have specified columns
        List<int> availableColumns = new List<int>();
        for (int i = 0; i < gridManager._width; i++)
        {
            availableColumns.Add(i);
        }

        // Spawn monsters
        for (int i = 0; i < wave.monsters.Count; i++)
        {
            MonsterSpawnData monsterData = wave.monsters[i];
            
            // Determine spawn column
            int column;
            if (monsterData.spawnColumn >= 0 && monsterData.spawnColumn < gridManager._width)
            {
                column = monsterData.spawnColumn;
            }
            else
            {
                // Auto-assign column if not specified
                if (availableColumns.Count > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, availableColumns.Count);
                    column = availableColumns[randomIndex];
                    availableColumns.RemoveAt(randomIndex);
                }
                else
                {
                    Debug.LogWarning($"Not enough columns available for monster {i} in wave {waveIndex}");
                    continue;
                }
            }

            SpawnMonster(column, monsterData.monsterPrefabIndex, monsterData.hp, monsterData.damage, monsterData.columnWidth);
        }
    }

    public void SpawnMonster(int column, int monsterPrefabIndex, int hp, int damage, int columnWidth)
    {
        if (gridManager == null)
        {
            Debug.LogError("GridManager not found!");
            return;
        }

        // Calculate spawn position
        Vector2 spawnPos = GetMonsterSpawnPosition(column);

        // Get monster prefab by index
        GameObject monsterPrefab = null;
        if (monsterPrefabs != null && monsterPrefabs.Length > 0)
        {
            if (monsterPrefabIndex >= 0 && monsterPrefabIndex < monsterPrefabs.Length)
            {
                monsterPrefab = monsterPrefabs[monsterPrefabIndex];
            }
            else
            {
                Debug.LogWarning($"Monster prefab index {monsterPrefabIndex} is out of range. Using first prefab.");
                monsterPrefab = monsterPrefabs[0];
            }
        }

        if (monsterPrefab == null)
        {
            Debug.LogError("No monster prefab available!");
            return;
        }

        GameObject monsterObj;

        if (ObjectPooler.Instance != null)
        {
            // Dùng một pool chung với tag "Monster"
            monsterObj = ObjectPooler.Instance.GetObject("Monster", spawnPos, Quaternion.identity);

            // Nếu pool chưa có hoặc hết object, fallback sang Instantiate prefab gốc
            if (monsterObj == null)
            {
                monsterObj = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
            }
        }
        else
        {
            monsterObj = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
        }

        // Initialize monster
        Monster monster = monsterObj.GetComponent<Monster>();
        if (monster == null)
        {
            monster = monsterObj.AddComponent<Monster>();
        }

        // Áp dụng visual / animator từ prefab template sang instance được lấy từ pool chung
        SpriteRenderer prefabSR = monsterPrefab.GetComponent<SpriteRenderer>();
        SpriteRenderer instanceSR = monsterObj.GetComponent<SpriteRenderer>();
        if (prefabSR != null && instanceSR != null)
        {
            instanceSR.sprite = prefabSR.sprite;
        }

        Animator prefabAnim = monsterPrefab.GetComponent<Animator>();
        Animator instanceAnim = monsterObj.GetComponent<Animator>();
        if (prefabAnim != null && instanceAnim != null)
        {
            instanceAnim.runtimeAnimatorController = prefabAnim.runtimeAnimatorController;
        }

        monster.Init(hp, damage, column, columnWidth);
        activeMonsters.Add(monster);

        // Register with CombatManager
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.RegisterMonster(monster);
        }
    }

    private Vector2 GetMonsterSpawnPosition(int column)
    {
        if (gridManager == null)
        {
            Debug.LogError("GridManager not found!");
            return Vector2.zero;
        }

        // Use GridManager's method if available, otherwise calculate manually
        return gridManager.GetMonsterSpawnPosition(column, spawnOffsetY);
    }

    private void HandleMonsterDied(Monster monster)
    {
        if (activeMonsters.Contains(monster))
        {
            activeMonsters.Remove(monster);
        }

        // Check if current wave is cleared
        if (activeMonsters.Count == 0)
        {
            OnWaveCleared?.Invoke(currentWaveIndex);

            // Spawn next wave or end
            if (currentWaveIndex + 1 < waves.Count)
            {
                StartCoroutine(SpawnNextWaveWhenIdle(currentWaveIndex + 1));
            }
            else
            {
                OnAllWavesCleared?.Invoke();
            }
        }
    }

    private IEnumerator SpawnNextWaveWhenIdle(int nextWaveIndex)
    {
        // Wait a bit before checking state
        yield return new WaitForSeconds(0.5f);
        
        // Check if board is already in IdleState
        if (gridManager != null && gridManager.StateManager != null && gridManager.StateManager.IsIdleState())
        {
            // Board is already idle, spawn immediately
            SpawnWave(nextWaveIndex);
        }
        else
        {
            // Board is not idle, wait for IdleState
            waitingForIdleState = true;
            pendingWaveIndex = nextWaveIndex;
            Debug.Log($"Waiting for board to enter IdleState before spawning wave {nextWaveIndex + 1}");
        }
    }

    public int GetCurrentWaveIndex() => currentWaveIndex;
    public int GetTotalWaves() => waves.Count;
    public int GetActiveMonsterCount() => activeMonsters.Count;
    public List<Monster> GetActiveMonsters() => new List<Monster>(activeMonsters);
}

