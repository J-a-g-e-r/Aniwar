using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckingState : IBoardState
{
    private GridManager board;
    public CheckingState(GridManager board)
    {
        this.board = board;
    }
    public void Enter()
    {
        bool hasMatch = CheckBoard();
        if (hasMatch)
        {
            board.StateManager.ChangeState(new DestroyingState(board));
        }
        else
        {
            board.StateManager.ChangeState(new ReSwappingState(board,board.SelectedGem,board.TargetGem));
        }
    }
    public void Execute()
    {
    }

    public void Exit()
    {
    }

    private bool CheckBoard()
    {
        bool foundMatch = false;

        for (int x = 0; x < board._width; x++)
        {
            for (int y = 0; y < board._height; y++)
            {
                GameObject obj = board._allGems[x, y];
                if (obj == null) continue;

                Gem gem = obj.GetComponent<Gem>();
                if (gem == null || gem.Variant == null) continue;

                // Ngang
                if (x > 0 && x < board._width - 1)
                {
                    Gem left = board._allGems[x - 1, y]?.GetComponent<Gem>();
                    Gem right = board._allGems[x + 1, y]?.GetComponent<Gem>();

                    if (left != null && right != null &&
                        left.Variant.color == gem.Variant.color &&
                        right.Variant.color == gem.Variant.color)
                    {
                        gem.SetMatched(true);
                        left.SetMatched(true);
                        right.SetMatched(true);
                        foundMatch = true;
                    }
                }

                // Doc
                if (y > 0 && y < board._height - 1)
                {
                    Gem down = board._allGems[x, y - 1]?.GetComponent<Gem>();
                    Gem up = board._allGems[x, y + 1]?.GetComponent<Gem>();

                    if (down != null && up != null &&
                        down.Variant.color == gem.Variant.color &&
                        up.Variant.color == gem.Variant.color)
                    {
                        gem.SetMatched(true);
                        down.SetMatched(true);
                        up.SetMatched(true);
                        foundMatch = true;
                    }
                }
            }
        }

        return foundMatch;
    }
}
