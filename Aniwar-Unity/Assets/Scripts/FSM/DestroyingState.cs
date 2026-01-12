using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyingState : IBoardState
{
    private GridManager board;
    private List<Gem> matchedGems = new();
    public DestroyingState(GridManager board)
    {
        this.board = board;
    }
    public void Enter()
    {
        board.EnableInput(false);
        CollectMatchedGems();
        board.StartCoroutine(DestroyCoroutine());
        

    }

    public void Execute()
    {
    }

    public void Exit()
    {
    }


    private void CollectMatchedGems()
    {
        matchedGems.Clear();
        for (int x = 0; x < board._width; x++)
        {
            for (int y = 0; y < board._height; y++)
            {
                GameObject obj = board._allGems[x, y];
                if (obj == null) continue;

                Gem gem = obj.GetComponent<Gem>();
                if (gem != null && gem.isMatched)
                {
                    matchedGems.Add(gem);
                }
            }
        }
    }


    private IEnumerator DestroyCoroutine()
    {
        yield return new WaitForSeconds(0.15f);

        // Lặp nhiều lượt để đảm bảo chain special được kích hoạt hết (ví dụ ngang kích hoạt dọc)
        while (true)
        {
            CollectMatchedGems();
            if (matchedGems.Count == 0)
                break;
            // Tìm tất cả các group match và tạo special cho mỗi group đủ điều kiện
            List<List<Gem>> allGroups = GetAllMatchGroups();
            HashSet<Gem> gemsToSkip = new HashSet<Gem>(); // Gem đã được chọn làm center cho special

            // Xử lý từng group, ưu tiên group lớn hơn trước
            allGroups.Sort((a, b) => b.Count.CompareTo(a.Count));

            foreach (List<Gem> group in allGroups)
            {
                if (group.Count == 0) continue;

                GemType specialType = GetSpecialType(group.Count);
                if (specialType == GemType.Normal) continue; // Không tạo special cho match 3

                // Tìm center gem trong group (ưu tiên SelectedGem nếu có trong group)
                Gem centerGem = null;
                
                // Ưu tiên gem được người chơi swap nếu có trong group
                if (board.SelectedGem != null && group.Contains(board.SelectedGem) && !gemsToSkip.Contains(board.SelectedGem))
                {
                    centerGem = board.SelectedGem;
                }
                else
                {
                    // Nếu không có SelectedGem, chọn gem ở giữa group
                    if (group.Count > 0)
                    {
                        int centerIndex = group.Count / 2;
                        for (int i = 0; i < group.Count; i++)
                        {
                            int index = (centerIndex + i) % group.Count;
                            Gem candidate = group[index];
                            if (!gemsToSkip.Contains(candidate))
                            {
                                centerGem = candidate;
                                break;
                            }
                        }
                    }
                }

                // Nếu không tìm được center gem (tất cả đã được chọn), bỏ qua group này
                if (centerGem == null) continue;

                GemVariant specialVariant = null;
                if (centerGem.Variant != null)
                {
                    // ColorExplode có màu đặc biệt, không phụ thuộc vào màu của gem match
                    if (specialType == GemType.ColorExplode)
                    {
                        specialVariant = board.GetColorExplodeVariant();
                    }
                    else
                    {
                        specialVariant = board.GetSpecialVariant(
                            centerGem.Variant.color,
                            specialType
                        );
                    }
                }

                // Nếu không tìm được variant, bỏ qua group này
                if (specialVariant == null) continue;

                // Đánh dấu center gem để không chọn lại
                gemsToSkip.Add(centerGem);
            }

            // Destroy tất cả gem matched (trừ các center gem đã được chọn)
            foreach (Gem gem in matchedGems)
            {
                if (gemsToSkip.Contains(gem)) continue; // Bỏ qua center gem, sẽ xử lý riêng

                int x = gem.column;
                int y = gem.row;
                board._allGems[x, y] = null;
                gem.DestroyGem();
            }

            // Destroy center gem và spawn special gem cho mỗi group đủ điều kiện
            foreach (Gem centerGem in gemsToSkip)
            {
                int x = centerGem.column;
                int y = centerGem.row;

                // Tìm lại group và special variant cho center gem này
                List<Gem> group = allGroups.Find(g => g.Contains(centerGem));
                if (group == null) continue;

                GemType specialType = GetSpecialType(group.Count);
                GemVariant specialVariant = null;

                if (specialType == GemType.ColorExplode)
                {
                    specialVariant = board.GetColorExplodeVariant();
                }
                else if (centerGem.Variant != null)
                {
                    specialVariant = board.GetSpecialVariant(
                        centerGem.Variant.color,
                        specialType
                    );
                }

                if (specialVariant != null)
                {
                    board._allGems[x, y] = null;
                    centerGem.DestroyGem();
                    board.SpawnSpecialGem(x, y, specialVariant);
                }
            }

            // Cho special effect (nếu có) đánh dấu isMatched xong trước khi vòng lặp kế tiếp thu thập
            yield return null;
        }
       
        yield return new WaitForEndOfFrame();

        board.DeselectGem();
        board.StateManager.ChangeState(new RefillingState(board));
    }

    private GemType GetSpecialType(int count)
    {
        if (count == 4)
            return Random.value < 0.5f ? GemType.HorizontalExplode : GemType.VerticalExplode;

        if (count == 5) 
        {
            AudioManager.Instance.CreateChocolate();
            return GemType.ColorExplode;
        }

        if (count >= 6)
        {
            AudioManager.Instance.WrapCandy();
            return GemType.AreaExplode;
        }

        return GemType.Normal;
    }

    private Gem GetCenterGem(List<Gem> group)
    {
        if (board.SelectedGem != null && group.Contains(board.SelectedGem))
            return board.SelectedGem;

        if (group != null && group.Count > 0)
            return group[group.Count / 2];

        return matchedGems.Count > 0 ? matchedGems[matchedGems.Count / 2] : null;
    }

    // Tìm tất cả các group match; xử lý cả giao nhau (T/L) bằng cách gộp group ngang + dọc cùng màu có chung ô
    private List<List<Gem>> GetAllMatchGroups()
    {
        List<List<Gem>> groups = new();

        // Quét ngang
        for (int y = 0; y < board._height; y++)
        {
            int x = 0;
            while (x < board._width)
            {
                Gem startGem = board._allGems[x, y]?.GetComponent<Gem>();
                if (startGem == null || !startGem.isMatched || startGem.Variant == null)
                {
                    x++;
                    continue;
                }

                List<Gem> current = new();
                GemColor color = startGem.Variant.color;
                int scanX = x;
                while (scanX < board._width)
                {
                    Gem g = board._allGems[scanX, y]?.GetComponent<Gem>();
                    if (g == null || !g.isMatched || g.Variant == null || g.Variant.color != color) break;
                    current.Add(g);
                    scanX++;
                }

                if (current.Count >= 3)
                    groups.Add(new List<Gem>(current));

                x = scanX;
            }
        }

        // Quét dọc
        for (int x = 0; x < board._width; x++)
        {
            int y = 0;
            while (y < board._height)
            {
                Gem startGem = board._allGems[x, y]?.GetComponent<Gem>();
                if (startGem == null || !startGem.isMatched || startGem.Variant == null)
                {
                    y++;
                    continue;
                }

                List<Gem> current = new();
                GemColor color = startGem.Variant.color;
                int scanY = y;
                while (scanY < board._height)
                {
                    Gem g = board._allGems[x, scanY]?.GetComponent<Gem>();
                    if (g == null || !g.isMatched || g.Variant == null || g.Variant.color != color) break;
                    current.Add(g);
                    scanY++;
                }

                if (current.Count >= 3)
                    groups.Add(new List<Gem>(current));

                y = scanY;
            }
        }

        // Gộp group cùng màu nếu có giao nhau (để nhận diện T/L)
        bool merged = true;
        while (merged)
        {
            merged = false;
            for (int i = 0; i < groups.Count; i++)
            {
                for (int j = i + 1; j < groups.Count; j++)
                {
                    if (CanMerge(groups[i], groups[j], out List<Gem> mergedGroup))
                    {
                        groups[i] = mergedGroup;
                        groups.RemoveAt(j);
                        merged = true;
                        break;
                    }
                }
                if (merged) break;
            }
        }

        return groups;
    }

    // Tìm group match lớn nhất (giữ lại để tương thích nếu có chỗ nào còn dùng)
    private List<Gem> GetLargestMatchGroup()
    {
        List<List<Gem>> allGroups = GetAllMatchGroups();
        List<Gem> bestGroup = new();
        foreach (var g in allGroups)
        {
            if (g.Count > bestGroup.Count)
                bestGroup = g;
        }
        return bestGroup;
    }

    // Hai group có thể gộp nếu cùng màu và có ít nhất một gem chung
    private bool CanMerge(List<Gem> a, List<Gem> b, out List<Gem> merged)
    {
        merged = null;
        if (a == null || b == null || a.Count == 0 || b.Count == 0)
            return false;

        // Kiểm tra màu
        GemVariant va = a[0].Variant;
        GemVariant vb = b[0].Variant;
        if (va == null || vb == null || va.color != vb.color)
            return false;

        // Kiểm tra giao nhau
        foreach (var ga in a)
        {
            if (b.Contains(ga))
            {
                merged = new List<Gem>(a);
                foreach (var gb in b)
                {
                    if (!merged.Contains(gb))
                        merged.Add(gb);
                }
                return true;
            }
        }
        return false;
    }

}
