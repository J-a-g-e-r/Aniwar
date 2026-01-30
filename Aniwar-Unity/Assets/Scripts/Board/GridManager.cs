using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

public class GridManager : MonoBehaviour
{

    //Thông tin bàn cờ
    [SerializeField] public int _width;
    [SerializeField] public int _height;
    [SerializeField] private float _space = 0.77f;
    [SerializeField] private GameObject _gemPrefabs;
    
    [Header("Level Configuration")]
    [SerializeField] private LevelData _levelData;
    
    // Loaded variants từ Addressables
    private List<GemVariant> _loadedGemVariants = new List<GemVariant>();
    private List<GemVariant> _loadedSpecialVariants = new List<GemVariant>();
    //private bool _variantsLoaded = false;
    
    [SerializeField] public GameObject[,] _allGems;
    [SerializeField] private GameObject _bulletPrefabs;


    // Animation settings
    [Header("Animation Settings")]
    [SerializeField] private float swapDuration = 0.3f;
    [SerializeField] private AnimationCurve swapCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float fallDuration = 0.5f;
    [SerializeField] private AnimationCurve fallCurve = AnimationCurve.EaseInOut(0.5f, 0, 1, 1);

    public Gem SelectedGem { get; private set;}
    public Gem TargetGem { get; set; }
    public BoardStateManager StateManager { get; private set; }
    private bool inputEnabled = false;

    private void Start()
    {
        _allGems = new GameObject[_width, _height];
        InitializeLevel().Forget();
    }

    private async UniTaskVoid InitializeLevel()
    {
        if (_levelData == null)
        {
            Debug.LogError("LevelData is not assigned!");
            return;
        }

        // Lấy variants đã được preload từ LevelLoaderManager
        if (LevelLoaderManager.Instance != null)
        {
            _loadedGemVariants = LevelLoaderManager.Instance.GetLoadedGemVariants(_levelData);
            _loadedSpecialVariants = LevelLoaderManager.Instance.GetLoadedSpecialVariants(_levelData);
        }
        else
        {
            Debug.LogError("LevelLoaderManager.Instance is null! Falling back to loading from Addressables...");
            // Fallback: load lại nếu không có preload
            await LoadGemVariantsFromAddressables();
        }

        // Cập nhật width và height từ LevelData
        if (_levelData.width > 0) _width = _levelData.width;
        if (_levelData.height > 0) _height = _levelData.height;
        _allGems = new GameObject[_width, _height];

        if (_loadedGemVariants.Count == 0)
        {
            Debug.LogError("No gem variants loaded!");
            return;
        }

        SetUp();
        StateManager = new BoardStateManager();
        StateManager.ChangeState(new IdleState(this));
    }

    private IEnumerator LoadGemVariantsFromAddressables()
    {
        _loadedGemVariants.Clear();
        _loadedSpecialVariants.Clear();
        
        // Load normal gem variants
        if (_levelData.gemVariantAddresses != null && _levelData.gemVariantAddresses.Count > 0)
        {
            Debug.Log($"Loading {_levelData.gemVariantAddresses.Count} gem variants from Addressables...");
            foreach (string address in _levelData.gemVariantAddresses)
            {
                var handle = Addressables.LoadAssetAsync<GemVariant>(address);
                yield return handle;
                
                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                {
                    _loadedGemVariants.Add(handle.Result);
                    Debug.Log($"Successfully loaded gem variant: {address}");
                }
                else
                {
                    Debug.LogError($"Failed to load gem variant at address: {address}. Status: {handle.Status}");
                }
            }
        }
        else
        {
            Debug.LogWarning("LevelData has no gem variant addresses specified!");
        }
        
        // Load special gem variants
        if (_levelData.specialGemVariantAddresses != null && _levelData.specialGemVariantAddresses.Count > 0)
        {
            Debug.Log($"Loading {_levelData.specialGemVariantAddresses.Count} special gem variants from Addressables...");
            foreach (string address in _levelData.specialGemVariantAddresses)
            {
                var handle = Addressables.LoadAssetAsync<GemVariant>(address);
                yield return handle;
                
                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                {
                    _loadedSpecialVariants.Add(handle.Result);
                    Debug.Log($"Successfully loaded special gem variant: {address}");
                }
                else
                {
                    Debug.LogError($"Failed to load special gem variant at address: {address}. Status: {handle.Status}");
                }
            }
        }
        
        
        if (_loadedGemVariants.Count == 0)
        {
            Debug.LogError("No gem variants loaded from Addressables! Please check LevelData configuration and ensure Addressable keys are correct.");
        }
        else
        {
            Debug.Log($"Successfully loaded {_loadedGemVariants.Count} gem variants and {_loadedSpecialVariants.Count} special variants.");
        }
    }

    private void OnEnable()
    {
        Gem.OnGemClicked += GemClicked;
    }

    private void OnDisable()
    {
        Gem.OnGemClicked -= GemClicked;
    }

    private void GemClicked (Gem gem)
    {
        if (!inputEnabled) return;
        StateManager.CurrentState
            ?.GetType()
            ?.GetMethod("OnGemClicked")
            ?.Invoke(StateManager.CurrentState, new object[] { gem });
    }


    //Tạo bảng
    private void SetUp()
    {

        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                //Vị trí các gem trên màn hình dựa trên i,j
                Vector2 spawnPos = GetWorldPosition(i, j);
                
                // Tạo gem và đảm bảo không có match-3
                GameObject newGems = CreateGemWithoutMatch(i, j, spawnPos);
                newGems.transform.SetParent(this.transform);
                newGems.name = $"Gem ({i},{j})";
                _allGems[i, j] = newGems;

                // Vị trí các gem trên lưới
                Gem gemComponent = newGems.GetComponent<Gem>();
                if (gemComponent != null)
                {
                    gemComponent.SetGridPosition(i, j);
                }
            }
        }
    }

    // Tạo gem mà không tạo match-3
    private GameObject CreateGemWithoutMatch(int x, int y, Vector2 spawnPos)
    {
        List<GemColor> excludedColors = new List<GemColor>();

        // Kiểm tra ngang
        if (x >= 2)
        {
            Gem g1 = _allGems[x - 1, y]?.GetComponent<Gem>();
            Gem g2 = _allGems[x - 2, y]?.GetComponent<Gem>();

            if (g1 != null && g2 != null &&
                g1.Variant.color == g2.Variant.color)
            {
                excludedColors.Add(g1.Variant.color);
            }
        }

        // Kiểm tra dọc
        if (y >= 2)
        {
            Gem g1 = _allGems[x, y - 1]?.GetComponent<Gem>();
            Gem g2 = _allGems[x, y - 2]?.GetComponent<Gem>();

            if (g1 != null && g2 != null &&
                g1.Variant.color == g2.Variant.color)
            {
                excludedColors.Add(g1.Variant.color);
            }
        }

        // Chọn variant hợp lệ từ loaded variants
        List<GemVariant> candidates = new List<GemVariant>();
        
        foreach (var v in _loadedGemVariants)
        {
            if (!excludedColors.Contains(v.color))
                candidates.Add(v);
        }

        GemVariant selected =
            candidates.Count > 0
            ? candidates[Random.Range(0, candidates.Count)]
            : (_loadedGemVariants.Count > 0 ? _loadedGemVariants[Random.Range(0, _loadedGemVariants.Count)] : null);
        
        if (selected == null)
        {
            Debug.LogError("No gem variant available to spawn!");
            return null;
        }

        // Spawn gem
        GameObject gemObj = null;
        if (ObjectPooler.Instance != null)
        {
            gemObj = ObjectPooler.Instance.GetObject("Gem", spawnPos, Quaternion.identity);
        }
        
        // Fallback nếu pool không có object hoặc ObjectPooler không tồn tại
        if (gemObj == null)
        {
            GameObject prefab = GetGemPrefab();
            if (prefab == null)
            {
                Debug.LogError("Gem prefab is not assigned!");
                return null;
            }
            gemObj = Instantiate(prefab, spawnPos, Quaternion.identity);
        }

        if (gemObj == null)
        {
            Debug.LogError("Failed to create gem object!");
            return null;
        }

        Gem gem = gemObj.GetComponent<Gem>();
        if (gem == null)
        {
            Debug.LogError("Gem component not found on gem object!");
            return null;
        }
        
        gem.ResetState();
        gem.Init(selected);
        return gemObj;


    }

    // Tính vị trí thế giới từ tọa độ lưới
    public Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(-2.3f, -2.5f) + new Vector2(x, y) * _space;
    }

    // Di chuyển gem tới vị trí mới với animation rơi xuống
    public IEnumerator MoveGemToPosition(GameObject gemObj, Vector2 targetPos)
    {
        if (gemObj == null) yield break;
        Vector3 startPos = gemObj.transform.position;
        float elapsed = 0f;
        while (elapsed < fallDuration)
        {
            if (gemObj == null) yield break; // gem đã bị destroy
            elapsed += Time.deltaTime;
            float t = fallCurve.Evaluate(elapsed / fallDuration);
            gemObj.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        if (gemObj != null)
        {
            gemObj.transform.position = targetPos;
        }
    }

    public GemVariant GetRandomGemVariant()
    {
        if (_loadedGemVariants.Count == 0)
        {
            Debug.LogError("No gem variants available!");
            return null;
        }
        return _loadedGemVariants[Random.Range(0, _loadedGemVariants.Count)];
    }

    public GameObject GetGemPrefab()
    {
        return _gemPrefabs;
    }

    //Chọn gem
    public void SelectGem(Gem gem)
    {
        DeselectGem();
        SelectedGem = gem;
        SelectedGem.SetSelected(true);

    }

    //Hủy chọn gem
    public void DeselectGem()
    {
        if (SelectedGem != null)
        {
            SelectedGem.SetSelected(false);
            SelectedGem = null;

        }

    }

    // Coroutine để swap gem với animation
    public IEnumerator SwapRoutine(Gem gem1, Gem gem2, System.Action onComplete)
    {
        // Lưu vị trí thế giới ban đầu
        Vector3 pos1 = gem1.transform.position;
        Vector3 pos2 = gem2.transform.position;

        // Lưu vị trí lưới ban đầu
        Vector2Int pos1Grid = gem1.gridPosition;
        Vector2Int pos2Grid = gem2.gridPosition;

        // -------- LẦN SWAP ĐẦU TIÊN --------
        float elapsedTime = 0f;

        while (elapsedTime < swapDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / swapDuration;
            float curveValue = swapCurve.Evaluate(progress);

            // Interpolate positions
            gem1.transform.position = Vector3.Lerp(pos1, pos2, curveValue);
            gem2.transform.position = Vector3.Lerp(pos2, pos1, curveValue);

            yield return null;
        }

        // Đảm bảo vị trí cuối cùng chính xác
        gem1.transform.position = pos2;
        gem2.transform.position = pos1;

        // Cập nhật mảng lưới
        _allGems[pos1Grid.x, pos1Grid.y] = gem2.gameObject;
        _allGems[pos2Grid.x, pos2Grid.y] = gem1.gameObject;

        // Cập nhật vị trí ô lưới cho component Gem
        gem1.SetGridPosition(pos2Grid.x, pos2Grid.y);
        gem2.SetGridPosition(pos1Grid.x, pos1Grid.y);

        // Chờ 1 frame để hệ thống match (FindMatches trong Gem.Update) kịp xử lý
        yield return null;
        onComplete?.Invoke();
    }

    public void EnableInput(bool value)
    {
        inputEnabled = value;
    }

    public void SpawnSpecialGem(int x, int y, GemVariant variant)
    {
        GameObject gemObj = ObjectPooler.Instance.GetObject(
            "Gem",
            GetWorldPosition(x, y),
            Quaternion.identity
        );

        Gem gem = gemObj.GetComponent<Gem>();
        gem.Init(variant);
        gem.SetGridPosition(x, y);

        _allGems[x, y] = gemObj;
    }

    public GemVariant GetSpecialVariant(GemColor color, GemType type)
    {
        foreach (var v in _loadedSpecialVariants)
        {
            if (v.color == color && v.type == type)
                return v;
        }
        return null;
    }

    // Lấy ColorExplode variant với bất kỳ màu nào (vì ColorExplode có màu đặc biệt)
    public GemVariant GetColorExplodeVariant()
    {
        foreach (var v in _loadedSpecialVariants)
        {
            if (v.type == GemType.ColorExplode)
                return v;
        }
        return null;
    }
    
    // Public method để set LevelData từ bên ngoài (nếu cần)
    public void SetLevelData(LevelData levelData)
    {
        _levelData = levelData;
    }

    public Vector2 GetMonsterSpawnPosition(int column, float offSetY = 1.5f)
    {
        Vector2 basePos = GetWorldPosition(column, _height);
        return new Vector2(basePos.x, basePos.y + offSetY);
    }

    public float GetSpace()
    {
        return _space;
    }
}
