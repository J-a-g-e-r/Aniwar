using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostSelectionState : IBoardState
{
    private GridManager board;
    private BoostManager boostManager;
    
    public BoostSelectionState(GridManager board, BoostManager boostManager)
    {
        this.board = board;
        this.boostManager = boostManager;
    }
    
    public void Enter()
    {
        board.EnableInput(true);
        // Deselect any previously selected gem
        board.DeselectGem();
        Debug.Log("Chọn một gem để sử dụng boost!");
    }
    
    public void Execute()
    {
        // Check if boost selection was cancelled
        if (!boostManager.IsWaitingForGemSelection())
        {
            board.StateManager.ChangeState(new IdleState(board));
        }
    }
    
    public void Exit()
    {
        board.EnableInput(false);
        board.DeselectGem();
    }
    
    public void OnGemClicked(Gem gem)
    {
        if (gem == null) return;
        if (boostManager == null) return;
        
        // When gem is clicked in boost selection mode, execute the boost
        // Visual feedback: select the gem
        board.SelectGem(gem);
        
        // Execute the boost with the selected gem
        boostManager.ExecuteBoostWithGem(gem);
    }
}

