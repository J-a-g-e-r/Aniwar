
public class BoardStateManager
{
    public IBoardState CurrentState { get; private set; }
    public void ChangeState(IBoardState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void Execute()
    {     
        CurrentState?.Execute();
    }
}
