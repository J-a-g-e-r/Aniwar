using System;

public class BoardStateManager
{
    public static event Action OnIdleStateEntered;
    public static event Action OnRefillStateCompleted;
    public static event Action OnMatchStarted; // Khi bắt đầu một match (vào DestroyingState)
    public static event Action OnMatchChainEnded; // Khi chuỗi match kết thúc (không có match sau refill)
    public static event Action OnNewSwapStarted; // Khi người chơi bắt đầu swap mới (vào SwappingState từ IdleState)
    
    public IBoardState CurrentState { get; private set; }
    public void ChangeState(IBoardState newState)
    {
        IBoardState previousState = CurrentState;
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
        
        // Notify when starting a match
        if (newState is DestroyingState)
        {
            OnMatchStarted?.Invoke();
        }
        
        // Notify when match chain ends (no match after refill)
        if (newState is ReSwappingState)
        {
            OnMatchChainEnded?.Invoke();
        }
        
        // Notify when player starts a new swap (from IdleState)
        if (newState is SwappingState && previousState is IdleState)
        {
            OnNewSwapStarted?.Invoke();
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
