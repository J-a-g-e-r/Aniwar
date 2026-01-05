using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GridManager : MonoBehaviour
{

    //Thông tin bàn cờ
    [SerializeField] public int _width;
    [SerializeField] public int _height;
    [SerializeField] private float _space = 0.77f;
    [SerializeField] private GameObject _gemPrefabs;
    [SerializeField] private GemVariant[] _gemVariants;
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
        SetUp();
        StateManager = new BoardStateManager();
        StateManager.ChangeState(new IdleState(this));


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

        // Chọn variant hợp lệ
        List<GemVariant> candidates = new List<GemVariant>();
        foreach (var v in _gemVariants)
        {
            if (!excludedColors.Contains(v.color))
                candidates.Add(v);
        }

        GemVariant selected =
            candidates.Count > 0
            ? candidates[Random.Range(0, candidates.Count)]
            : _gemVariants[Random.Range(0, _gemVariants.Length)];

        // Spawn gem
        GameObject gemObj;
        if (ObjectPooler.Instance != null)
        {
            gemObj = ObjectPooler.Instance.GetObject("Gem", spawnPos, Quaternion.identity);
        }
        else
        {
            gemObj = Instantiate(GetGemPrefab(), spawnPos, Quaternion.identity);
        }

        Gem gem = gemObj.GetComponent<Gem>();
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
        //return _gemPrefabs[Random.Range(0, _gemPrefabs.Length)];
        return _gemVariants[Random.Range(0,_gemVariants.Length)] ;
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
        foreach (var v in _gemVariants)
        {
            if (v.color == color && v.type == type)
                return v;
        }
        return null;
    }

}
