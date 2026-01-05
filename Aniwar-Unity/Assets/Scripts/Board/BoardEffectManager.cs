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
            }
        }
    }
}
