using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardEffectManager : MonoBehaviour
{
    public static BoardEffectManager Instance;
    public GridManager board;
    private void Awake()
    {
        Instance = this;
    }

    public void ClearRow(int row)
    {
        for (int x = 0; x < board._width; x++)
        {
            Gem gem = board._allGems[x, row]?.GetComponent<Gem>();
            if (gem != null)
            {
                gem.SetMatched(true);
                AudioManager.Instance.LineBlast();

            }
        }
    }

    public void ClearColumn(int column)
    {
        for (int y = 0; y < board._height; y++)
        {
            Gem gem = board._allGems[column, y]?.GetComponent<Gem>();
            if (gem != null)
            {
                gem.SetMatched(true);
                AudioManager.Instance.LineBlast();
            }
        }
    }
    public void ClearArea(int column, int row, int radius)
    {
        for (int x = column - radius; x <= column + radius; x++)
        {
            for (int y = row - radius; y <= row + radius; y++)
            {
                if (x >= 0 && x < board._width && y >= 0 && y < board._height)
                {
                    Gem gem = board._allGems[x, y]?.GetComponent<Gem>();
                    if (gem != null)
                    {
                        gem.SetMatched(true);
                        AudioManager.Instance.Bomb();
                    }
                }
            }
        }
    }

    public void ClearColor(GemColor color)
    {
        for (int x = 0; x < board._width; x++)
        {
            for (int y = 0; y < board._height; y++)
            {
                Gem gem = board._allGems[x, y]?.GetComponent<Gem>();
                if (gem != null 
                    && gem.Variant != null 
                    && gem.Variant.color == color
                    // Không đánh dấu ColorExplode khác để tránh tự phá lẫn nhau
                    && gem.Variant.type != GemType.ColorExplode)
                {
                    gem.SetMatched(true);

                }
            }
        }
        AudioManager.Instance.ColorBomb();
    }

}
