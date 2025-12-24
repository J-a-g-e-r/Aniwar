using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwappingState : IBoardState
{
    private GridManager board;
    public SwappingState(GridManager board)
    {
        this.board = board;
    }
    public void Enter()
    {
        board.StartCoroutine(SwapRoutine());
    }

    public void Execute()
    {
    }

    public void Exit()
    {
    }

    private IEnumerator SwapRoutine()
    {
        yield return board.SwapRoutine(board.SelectedGem, board.TargetGem,OnSwapDone);
    }

    private void OnSwapDone()
    {

        board.StateManager.ChangeState(new CheckingState(board));
    }
}
