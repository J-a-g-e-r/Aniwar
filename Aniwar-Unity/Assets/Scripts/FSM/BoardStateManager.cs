using System;

public class BoardStateManager
{
    public static event Action OnIdleStateEntered;
    public static event Action OnRefillStateCompleted;
    
    public IBoardState CurrentState { get; private set; }
    public void ChangeState(IBoardState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
        
        // Notify when entering IdleState
        if (newState is IdleState)
        {
            OnIdleStateEntered?.Invoke();
            // Monsters tấn công sau khi refill xong và board vào IdleState
            OnRefillStateCompleted?.Invoke();
        }
    }

    public void Execute()
    {     
        CurrentState?.Execute();
    }
    
    public bool IsIdleState()
    {
        return CurrentState is IdleState;
    }
}
