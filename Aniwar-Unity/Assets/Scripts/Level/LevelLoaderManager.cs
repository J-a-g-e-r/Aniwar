using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Threading;

public class LevelLoaderManager : MonoBehaviour
{
    public static LevelLoaderManager Instance { get; private set; }

    // Cache các variants ð? load
    private Dictionary<string, GemVariant> _cachedGemVariants = new Dictionary<string, GemVariant>();
    private Dictionary<string, GemVariant> _cachedSpecialVariants = new Dictionary<string, GemVariant>();

    // LevelData hi?n t?i ðang ðý?c load
    private LevelData _currentLevelData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Preload t?t c? Addressables cho m?t level và tr? v? progress (0-1)
    /// </summary>
    public async UniTask<float> PreloadLevelAssetsAsync(LevelData levelData, System.Action<float> onProgress = null, CancellationToken ct = default)
    {
        _currentLevelData = levelData;

        int totalAssets = 0;
        int loadedAssets = 0;

        // Ð?m t?ng s? assets c?n load
        if (levelData.gemVariantAddresses != null)
            totalAssets += levelData.gemVariantAddresses.Count;
        if (levelData.specialGemVariantAddresses != null)
            totalAssets += levelData.specialGemVariantAddresses.Count;

        if (totalAssets == 0)
        {
            Debug.LogWarning("No assets to preload!");
            return 1f;
        }

        // Load normal gem variants
        if (levelData.gemVariantAddresses != null && levelData.gemVariantAddresses.Count > 0)
        {
            foreach (string address in levelData.gemVariantAddresses)
            {
                if (ct.IsCancellationRequested) return 0f;

                // Ki?m tra cache trý?c
                if (!_cachedGemVariants.ContainsKey(address))
                {
                    try
                    {
                        var variant = await Addressables.LoadAssetAsync<GemVariant>(address).ToUniTask(cancellationToken: ct);
                        _cachedGemVariants[address] = variant;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Failed to load gem variant at address: {address}. Error: {ex.Message}");
                    }
                }

                loadedAssets++;
                float progress = (float)loadedAssets / totalAssets;
                onProgress?.Invoke(progress);
            }
        }

        // Load special gem variants
        if (levelData.specialGemVariantAddresses != null && levelData.specialGemVariantAddresses.Count > 0)
        {
            foreach (string address in levelData.specialGemVariantAddresses)
            {
                if (ct.IsCancellationRequested) return 0f;

                // Ki?m tra cache trý?c
                if (!_cachedSpecialVariants.ContainsKey(address))
                {
                    try
                    {
                        var variant = await Addressables.LoadAssetAsync<GemVariant>(address).ToUniTask(cancellationToken: ct);
                        _cachedSpecialVariants[address] = variant;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Failed to load special gem variant at address: {address}. Error: {ex.Message}");
                    }
                }

                loadedAssets++;
                float progress = (float)loadedAssets / totalAssets;
                onProgress?.Invoke(progress);
            }
        }

        return 1f;
    }

    /// <summary>
    /// L?y danh sách variants ð? load cho level hi?n t?i
    /// </summary>
    public List<GemVariant> GetLoadedGemVariants(LevelData levelData)
    {
        List<GemVariant> variants = new List<GemVariant>();

        if (levelData?.gemVariantAddresses != null)
        {
            foreach (string address in levelData.gemVariantAddresses)
            {
                if (_cachedGemVariants.TryGetValue(address, out var variant))
                {
                    variants.Add(variant);
                }
            }
        }

        return variants;
    }

    /// <summary>
    /// L?y danh sách special variants ð? load cho level hi?n t?i
    /// </summary>
    public List<GemVariant> GetLoadedSpecialVariants(LevelData levelData)
    {
        List<GemVariant> variants = new List<GemVariant>();

        if (levelData?.specialGemVariantAddresses != null)
        {
            foreach (string address in levelData.specialGemVariantAddresses)
            {
                if (_cachedSpecialVariants.TryGetValue(address, out var variant))
                {
                    variants.Add(variant);
                }
            }
        }

        return variants;
    }

    /// <summary>
    /// Clear cache khi không c?n thi?t (ví d? khi quay v? menu)
    /// </summary>
    public void ClearCache()
    {
        foreach (var variant in _cachedGemVariants.Values)
        {
            Addressables.Release(variant);
        }
        foreach (var variant in _cachedSpecialVariants.Values)
        {
            Addressables.Release(variant);
        }

        _cachedGemVariants.Clear();
        _cachedSpecialVariants.Clear();
    }
}
