using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReSwappingState : IBoardState
{
    private GridManager board;
    private Gem gem1;
    private Gem gem2;

    public ReSwappingState(GridManager board, Gem gem1, Gem gem2)
    {
        this.board = board;
        this.gem1 = gem1;
        this.gem2 = gem2;
    }
    public void Enter()
    {
        if (gem1 == null || gem2 == null)
        {
            board.StateManager.ChangeState(new IdleState(board));
            return;
        }
        board.EnableInput(false);
        board.StartCoroutine(board.SwapRoutine(gem1, gem2, OnRevertDone));
    }

    public void Execute()
    {
    }

    public void Exit()
    {
    }

    private void OnRevertDone()
    {
        board.DeselectGem();
        board.StateManager.ChangeState(new IdleState(board));
    }
}
