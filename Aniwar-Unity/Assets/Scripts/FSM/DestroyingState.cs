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
        foreach (Gem gem in matchedGems)
        {
            int x = gem.column;
            int y = gem.row;
            board._allGems[x, y] = null;
            gem.DestroyGem();
        }
        yield return new WaitForEndOfFrame();
        board.DeselectGem();
        board.StateManager.ChangeState(new RefillingState(board));
    }
}
