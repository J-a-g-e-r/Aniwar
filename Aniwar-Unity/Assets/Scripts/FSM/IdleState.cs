using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : IBoardState
{
    private GridManager board;
    public IdleState(GridManager board)
    {
        this.board = board;
    }

    public void Enter()
    {
        board.EnableInput(true);
    }

    public void Execute()
    {
        
    }

    public void Exit()
    {
        board.EnableInput(false);
    }

    public void OnGemClicked(Gem gem)
    {
        if(gem == null) return;
        if(board.SelectedGem == null)
        {
            board.SelectGem(gem);
            return;
        }

        if (board.SelectedGem == gem)
        {
            board.DeselectGem();
            return;
        }

        board.TargetGem = gem;
        board.StateManager.ChangeState(new SwappingState(board));
    }
}
