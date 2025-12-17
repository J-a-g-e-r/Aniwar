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

    //Chứa gems prefabs
    [SerializeField] private GameObject[] _gemPrefabs;


    // Mảng 2D chứa các gem trên lưới
    [SerializeField] public GameObject[,] _allGems;

    // Lưu Gem được chọn    
    private Gem selectedGem = null;

    // Kiểm tra xem đang có đang trong trạng thái swap không -- tránh việc spam swap
    private bool isSwapping = false;
    // Trạng thái đang xử lý refill board
    private bool isFilling = false;
    
    // Animation settings
    [Header("Animation Settings")]
    [SerializeField] private float swapDuration = 0.3f;
    [SerializeField] private AnimationCurve swapCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float fallDuration = 0.2f;
    [SerializeField] private AnimationCurve fallCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);


   
    private void Start()
    {
        _allGems = new GameObject[_width, _height];
        SetUp();


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
                GameObject newGems = Instantiate(_gemPrefabs[Random.Range(0, _gemPrefabs.Length)], spawnPos, Quaternion.identity);
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

    // Tính vị trí thế giới từ tọa độ lưới
    private Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(-2.3f, -2.5f) + new Vector2(x, y) * _space;
    }

    // Di chuyển gem tới vị trí mới với animation rơi xuống
    private IEnumerator MoveGemToPosition(GameObject gemObj, Vector2 targetPos)
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

    //Hàm được gọi từ Gem khi gem bị phá hủy
    public void RemoveGem(Gem gem)
    {
        if (gem == null) return;
        if (_allGems == null) return;

        int c = gem.column;
        int r = gem.row;
        if (c >= 0 && c < _width && r >= 0 && r < _height)
        {
            if (_allGems[c, r] == gem.gameObject)
            {
                _allGems[c, r] = null;
            }
        }

        // Chỉ chạy một coroutine refill tại một thời điểm
        if (!isFilling)
        {
            StartCoroutine(FillBoardCoroutine());
        }
    }

    private IEnumerator FillBoardCoroutine()
    {
        isFilling = true;

        // Đợi hết frame hiện tại để gom đủ các gem bị destroy cùng lúc
        yield return new WaitForEndOfFrame();

        // Dồn các gem xuống dưới
        for (int x = 0; x < _width; x++)
        {
            int targetRow = 0; // hàng thấp nhất còn trống để đặt gem xuống
            for (int y = 0; y < _height; y++)
            {
                GameObject gemObj = _allGems[x, y];
                if (gemObj != null)
                {
                    if (y != targetRow)
                    {
                        // Cập nhật mảng và vị trí lưới
                        _allGems[x, targetRow] = gemObj;
                        _allGems[x, y] = null;
                        Gem g = gemObj.GetComponent<Gem>();
                        if (g != null)
                        {
                            g.SetGridPosition(x, targetRow);
                            g.isMatch = false; // reset trạng thái match
                        }
                        // Di chuyển với animation rơi
                        Vector2 targetPos = GetWorldPosition(x, targetRow);
                        StartCoroutine(MoveGemToPosition(gemObj, targetPos));
                    }
                    targetRow++;
                }
            }

            // Sau khi dồn xuống, spawn thêm gem mới cho các ô còn trống
            for (int y = targetRow; y < _height; y++)
            {
                Vector2 spawnPos = GetWorldPosition(x, _height + (y - targetRow)); // spawn cao hơn để tạo hiệu ứng rơi
                Vector2 targetPos = GetWorldPosition(x, y);

                GameObject newGem = Instantiate(_gemPrefabs[Random.Range(0, _gemPrefabs.Length)], spawnPos, Quaternion.identity);
                newGem.transform.SetParent(this.transform);
                newGem.name = $"Gem ({x},{y})";

                Gem gemComponent = newGem.GetComponent<Gem>();
                if (gemComponent != null)
                {
                    gemComponent.SetGridPosition(x, y);
                    gemComponent.isMatch = false;
                }

                _allGems[x, y] = newGem;

                // Animate rơi xuống vị trí đích
                StartCoroutine(MoveGemToPosition(newGem, targetPos));
            }
        }

        isFilling = false;
    }

    //Chọn gem
    public void SelectGem(Gem gem)
    {
        // Bỏ chọn gem cũ nếu có
        DeselectGem();

        // Chọn gem mới
        selectedGem = gem;
        selectedGem.SetSelected(true);


        // Highlight các gem có thể swap
        //HighlightSwappableGems();
    }
    
    //Hủy chọn gem
    public void DeselectGem()
    {
        if (selectedGem != null)
        {
            selectedGem.SetSelected(false);
            selectedGem = null;

        }

    }


    // Coroutine để swap gem với animation
    private IEnumerator SwapGemsCoroutine(Gem gem1, Gem gem2)
    {
            isSwapping = true;

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

            bool hasMatch = gem1.isMatch || gem2.isMatch;

            // Nếu KHÔNG tạo được match thì swap ngược lại (quay về vị trí cũ)
            if (!hasMatch)
            {
                elapsedTime = 0f;

                while (elapsedTime < swapDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float progress = elapsedTime / swapDuration;
                    float curveValue = swapCurve.Evaluate(progress);

                    // Đổi chỗ ngược lại
                    gem1.transform.position = Vector3.Lerp(pos2, pos1, curveValue);
                    gem2.transform.position = Vector3.Lerp(pos1, pos2, curveValue);

                    yield return null;
                }

                // Đảm bảo vị trí cuối cùng chính xác (quay về chỗ cũ)
                gem1.transform.position = pos1;
                gem2.transform.position = pos2;

                // Cập nhật lại mảng lưới
                _allGems[pos1Grid.x, pos1Grid.y] = gem1.gameObject;
                _allGems[pos2Grid.x, pos2Grid.y] = gem2.gameObject;

                // Cập nhật lại vị trí ô lưới
                gem1.SetGridPosition(pos1Grid.x, pos1Grid.y);
                gem2.SetGridPosition(pos2Grid.x, pos2Grid.y);
            }

            // Bỏ chọn gem sau khi hoàn thành xử lý
            DeselectGem();

            isSwapping = false;

    }

    // Method để đổi chỗ gem khi được click
    public void SwapGems(Gem clickedGem)
    {
        if (isSwapping) return;
        
        if (selectedGem == null)
        {
            // Chọn gem đó
            SelectGem(clickedGem);
        }
        else if (selectedGem == clickedGem)
        {
            // Bỏ chọn gem hiện tại
            DeselectGem();
        }
        else
        {
            // Swap với gem đã chọn
            if (isSwapping) return;

            StartCoroutine(SwapGemsCoroutine(selectedGem, clickedGem));
            
        }
    }



}
